using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;

namespace Dtd.Domain.Documentos;

public sealed class DocumentoDigitalTransporte : Entity<Guid>
{
    private readonly List<Expedicion> _expediciones = [];
    private readonly List<ConductorAsignado> _conductores = [];
    private readonly List<CcAsignado> _ccs = [];
    private readonly List<Envio> _envios = [];

    public string Empresa { get; private set; }
    public Guid AlmacenId { get; private set; }
    public Guid AgenciaId { get; private set; }

    public OrigenDocumento Origen { get; private set; }
    public RangoFechas RangoFechas { get; private set; }

    public EstadoDocumento Estado { get; private set; }
    public TipoAgrupacionEnvio TipoAgrupacion { get; private set; }
    public string? PlataformaId { get; private set; }
    public string? PlataformaEstado { get; private set; }

    public string? UsuarioGeneracionId { get; private set; }
    public DateTimeOffset FechaGeneracion { get; private set; }

    public IReadOnlyCollection<Expedicion> Expediciones => _expediciones;
    public IReadOnlyCollection<Envio> Envios => _envios;
    public IReadOnlyCollection<ConductorAsignado> Conductores => _conductores;
    public IReadOnlyCollection<CcAsignado> Ccs => _ccs;

    public string Referencia { get; private set; } = string.Empty;

    private DocumentoDigitalTransporte()
    {
        Empresa = string.Empty;
        Origen = null!;
        RangoFechas = null!;
    }

    private DocumentoDigitalTransporte(
        string empresa,
        string referencia,
        Guid almacenId,
        Guid agenciaId,
        OrigenDocumento origen,
        RangoFechas rangoFechas,
        TipoAgrupacionEnvio tipoAgrupacion,
        string? usuarioGeneracionId,
        DateTimeOffset fechaGeneracion)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException(
                "La empresa es obligatoria.",
                nameof(empresa));
        }

        if (string.IsNullOrWhiteSpace(referencia))
        {
            throw new ArgumentException(
                "La referencia del documento es obligatoria.",
                nameof(referencia));
        }

        if (almacenId == Guid.Empty)
        {
            throw new ArgumentException(
                "El almacén es obligatorio.",
                nameof(almacenId));
        }

        if (agenciaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La agencia es obligatoria.",
                nameof(agenciaId));
        }

        Id = Guid.NewGuid();

        Empresa = empresa.Trim();
        Referencia = referencia.Trim();
        AlmacenId = almacenId;
        AgenciaId = agenciaId;

        Origen = origen
            ?? throw new ArgumentNullException(nameof(origen));

        RangoFechas = rangoFechas
            ?? throw new ArgumentNullException(nameof(rangoFechas));

        TipoAgrupacion = tipoAgrupacion;

        UsuarioGeneracionId = usuarioGeneracionId;
        FechaGeneracion = fechaGeneracion;

        Estado = EstadoDocumento.Nuevo;
    }

    public static ErrorOr<DocumentoDigitalTransporte> Generar(
    string empresa,
    string referencia,
    Guid almacenId,
    Guid agenciaId,
    OrigenDocumento origen,
    RangoFechas rangoFechas,
    IReadOnlyCollection<Expedicion> expediciones,
    TipoAgrupacionEnvio tipoAgrupacion,
    DestinoEnvio? destinoAgencia,
    IReadOnlyDictionary<string, DestinoEnvio> destinosAlmacen,
    string? usuarioGeneracionId,
    DateTimeOffset fechaGeneracion)
    {
        ArgumentNullException.ThrowIfNull(expediciones);
        ArgumentNullException.ThrowIfNull(destinosAlmacen);

        if (string.IsNullOrWhiteSpace(referencia))
        {
            throw new ArgumentException(
                "La referencia del documento es obligatoria.",
                nameof(referencia));
        }

        if (expediciones.Count == 0)
        {
            return Error.Validation(
                "Documento.SinExpediciones",
                "No se puede generar un documento sin expediciones.");
        }

        var expedicionesDuplicadas = expediciones
            .GroupBy(
                e => e.ErpId,
                StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (expedicionesDuplicadas.Count > 0)
        {
            return Error.Validation(
                "Documento.ExpedicionDuplicada",
                "Las siguientes expediciones ERP están duplicadas: " +
                $"{string.Join(", ", expedicionesDuplicadas)}.");
        }

        var documento = new DocumentoDigitalTransporte(
            empresa,
            referencia,
            almacenId,
            agenciaId,
            origen,
            rangoFechas,
            tipoAgrupacion,
            usuarioGeneracionId,
            fechaGeneracion);

        documento._expediciones.AddRange(expediciones);

        var resultadoEnvios = documento.GenerarEnvios(
            destinoAgencia,
            destinosAlmacen);

        if (resultadoEnvios.IsError)
        {
            return resultadoEnvios.Errors;
        }

        return documento;
    }

    private ErrorOr<Success> GenerarEnvios(
    DestinoEnvio? destinoAgencia,
    IReadOnlyDictionary<string, DestinoEnvio> destinosAlmacen)
    {
        return TipoAgrupacion switch
        {
            TipoAgrupacionEnvio.UnicoPorAgencia
                => GenerarEnvioUnicoPorAgencia(destinoAgencia),

            TipoAgrupacionEnvio.PorAlmacenDestino
                => GenerarEnviosPorAlmacenDestino(destinosAlmacen),

            _ => Error.Validation(
                "Documento.TipoAgrupacionNoSoportado",
                $"El tipo de agrupación '{TipoAgrupacion}' no está soportado.")
        };
    }

    private ErrorOr<Success> GenerarEnvioUnicoPorAgencia(
    DestinoEnvio? destinoAgencia)
    {
        if (destinoAgencia is null)
        {
            return Error.Validation(
                "Documento.DestinoAgenciaRequerido",
                "No está configurado el destino base de la agencia.");
        }

        var envio = Envio.Crear(
            orden: 1,
            referencia: GenerarReferenciaEnvio(1),
            bultos: _expediciones.Sum(e => e.Bultos),
            destino: destinoAgencia);

        foreach (var expedicion in _expediciones)
        {
            expedicion.AsignarAEnvio(envio.Id);
        }

        _envios.Add(envio);

        return Result.Success;
    }

    private ErrorOr<Success> GenerarEnviosPorAlmacenDestino(
        IReadOnlyDictionary<string, DestinoEnvio> destinosAlmacen)
    {
        var expedicionCliente = _expediciones
            .FirstOrDefault(
                e => e.ExpeditionType == Expedicion.TipoCliente);

        if (expedicionCliente is not null)
        {
            return Error.Validation(
                "Documento.ExpedicionNoValidaParaDestinoAlmacen",
                $"La expedición ERP '{expedicionCliente.ErpId}' " +
                "no es un trasiego y no puede agruparse por almacén destino.");
        }

        var expedicionSinDestino = _expediciones
            .FirstOrDefault(
                e => string.IsNullOrWhiteSpace(
                    e.Destino.AlmacenDestino));

        if (expedicionSinDestino is not null)
        {
            return Error.Validation(
                "Documento.AlmacenDestinoRequerido",
                $"La expedición ERP '{expedicionSinDestino.ErpId}' " +
                "no tiene almacén destino.");
        }

        var codigosDestino = _expediciones
            .Select(e => e.Destino.AlmacenDestino!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(codigo => codigo)
            .ToList();

        var almacenesNoConfigurados = codigosDestino
            .Where(codigo =>
                !destinosAlmacen.ContainsKey(codigo))
            .ToList();

        if (almacenesNoConfigurados.Count > 0)
        {
            return Error.Validation(
                "Documento.AlmacenDestinoNoConfigurado",
                $"Los siguientes almacenes destino no están configurados: " +
                $"{string.Join(", ", almacenesNoConfigurados)}.");
        }

        var grupos = _expediciones
            .GroupBy(
                e => e.Destino.AlmacenDestino!,
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key);

        var orden = 1;

        foreach (var grupo in grupos)
        {
            var envio = Envio.Crear(
                orden: orden,
                referencia: GenerarReferenciaEnvio(orden),
                bultos: grupo.Sum(e => e.Bultos),
                destino: destinosAlmacen[grupo.Key]);

            foreach (var expedicion in grupo)
            {
                expedicion.AsignarAEnvio(envio.Id);
            }

            _envios.Add(envio);

            orden++;
        }

        return Result.Success;
    }

    private string GenerarReferenciaEnvio(int orden)
    {
        return $"{Id}#{orden}";
    }

    public void AsignarConductor(
        ConductorAsignado conductor)
    {
        ArgumentNullException.ThrowIfNull(conductor);

        AsegurarEstadoNuevo();

        if (_conductores.Any(
            c => c.ConductorCatalogId ==
                 conductor.ConductorCatalogId))
        {
            return;
        }

        _conductores.Add(conductor);
    }

    public void RemoverConductor(
        Guid conductorId)
    {
        AsegurarEstadoNuevo();

        var conductor = _conductores
            .FirstOrDefault(c => c.Id == conductorId)
            ?? throw new InvalidOperationException(
                $"El conductor '{conductorId}' " +
                "no está asignado al documento.");

        _conductores.Remove(conductor);
    }

    public void AsignarCc(
        CcAsignado cc)
    {
        ArgumentNullException.ThrowIfNull(cc);

        AsegurarEstadoNuevo();

        if (_ccs.Any(
            c => c.CcCatalogId == cc.CcCatalogId))
        {
            return;
        }

        _ccs.Add(cc);
    }

    public void RemoverCc(
        Guid ccId)
    {
        AsegurarEstadoNuevo();

        var cc = _ccs
            .FirstOrDefault(c => c.Id == ccId)
            ?? throw new InvalidOperationException(
                $"El CC '{ccId}' " +
                "no está asignado al documento.");

        _ccs.Remove(cc);
    }

    public ErrorOr<Success> ValidarListoParaEnviar()
    {
        if (Estado != EstadoDocumento.Nuevo)
        {
            return Error.Conflict(
                "Documento.YaConfirmado",
                $"El documento ya está en estado '{Estado}' " +
                "y no se puede enviar.");
        }

        if (_envios.Count == 0)
        {
            return Error.Failure(
                "Documento.SinEnvios",
                "El documento no contiene ningún envío.");
        }

        if (_conductores.Count == 0)
        {
            return Error.Validation(
                "Documento.ConductorRequerido",
                "El documento no tiene ningún conductor asignado.");
        }

        if (_conductores.Any(c => !c.TieneCanalValido))
        {
            return Error.Validation(
                "Documento.ConductorSinCanal",
                "Algún conductor asignado no tiene " +
                "un canal de comunicación válido.");
        }

        var envioSinDestino = _envios
            .FirstOrDefault(e => !e.TieneDestinoValido);

        if (envioSinDestino is not null)
        {
            return Error.Validation(
                "Documento.EnvioSinDestino",
                $"El envío '{envioSinDestino.Referencia}' no tiene destino.");
        }

        return Result.Success;
    }

    public void ConfirmarEnvioADocuten(string lotId, string estadoDocuten)
    {
        if (Estado != EstadoDocumento.Nuevo)
        {
            throw new InvalidOperationException(
                $"El documento ya está en estado '{Estado}' y no se puede reenviar.");
        }

        PlataformaId = NormalizarOpcional(lotId);
        PlataformaEstado = NormalizarOpcional(estadoDocuten);
        Estado = EstadoDocumento.Enviando;
    }

    public bool RegistrarCallbackDocumentoDocuten(string? lotId, string? estadoDocuten)
    {
        if (!string.IsNullOrWhiteSpace(lotId))
        {
            if (!string.IsNullOrWhiteSpace(PlataformaId) &&
                !string.Equals(PlataformaId, lotId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            PlataformaId = lotId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(estadoDocuten))
        {
            var estado = estadoDocuten.Trim();
            PlataformaEstado = estado;
            AplicarEstadoDocumentoDesdeCallbackPlataforma(estado);
        }

        if (Estado == EstadoDocumento.Nuevo)
        {
            Estado = EstadoDocumento.Enviando;
        }

        return true;
    }

    private void AplicarEstadoDocumentoDesdeCallbackPlataforma(string estado)
    {
        if (string.Equals(estado, EstadoDocuten.Success, StringComparison.OrdinalIgnoreCase))
        {
            if (Estado is EstadoDocumento.Nuevo or EstadoDocumento.Enviando or EstadoDocumento.Error)
            {
                Estado = EstadoDocumento.PendienteFirmas;
            }

            return;
        }

        if (string.Equals(estado, EstadoDocuten.Error, StringComparison.OrdinalIgnoreCase))
        {
            if (Estado is not EstadoDocumento.Finalizado and not EstadoDocumento.Anulado and not EstadoDocumento.Cancelado)
            {
                Estado = EstadoDocumento.Error;
            }

            return;
        }

        if (string.Equals(estado, EstadoDocuten.Completed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(estado, EstadoDocuten.Delivered, StringComparison.OrdinalIgnoreCase))
        {
            if (Estado is not EstadoDocumento.Anulado and not EstadoDocumento.Cancelado)
            {
                Estado = EstadoDocumento.Finalizado;
            }

            return;
        }

        if (string.Equals(estado, EstadoDocuten.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            if (Estado is not EstadoDocumento.Finalizado and not EstadoDocumento.Anulado)
            {
                Estado = EstadoDocumento.Cancelado;
            }
        }
    }

    public bool ConfirmarEnvioPlataforma(
        string shipmentReference,
        string? shipmentId,
        string? estado)
    {
        if (string.IsNullOrWhiteSpace(shipmentReference))
        {
            return false;
        }

        var envio = _envios.FirstOrDefault(e =>
            string.Equals(e.Referencia, shipmentReference.Trim(), StringComparison.OrdinalIgnoreCase));

        if (envio is null)
        {
            return false;
        }

        envio.ConfirmarEnvioPlataforma(shipmentId, estado);
        RecalcularEstadoDesdeEnviosPlataforma();
        return true;
    }

    public bool RegistrarCallbackEnvioDocuten(
        string shipmentReference,
        string? shipmentId,
        string? estadoDocuten)
    {
        if (string.IsNullOrWhiteSpace(shipmentReference))
        {
            return false;
        }

        var envio = _envios.FirstOrDefault(e =>
            string.Equals(e.Referencia, shipmentReference.Trim(), StringComparison.OrdinalIgnoreCase));

        if (envio is null)
        {
            return false;
        }

        envio.RegistrarCallbackDocuten(shipmentId, estadoDocuten);
        RecalcularEstadoDesdeEnviosPlataforma();
        return true;
    }

    private void RecalcularEstadoDesdeEnviosPlataforma()
    {
        if (Estado is EstadoDocumento.Anulado or EstadoDocumento.Cancelado)
        {
            return;
        }

        var estados = _envios
            .Select(e => e.PlataformaEnvioEstado)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!)
            .ToList();

        if (estados.Count == 0)
        {
            return;
        }

        if (estados.Any(e => string.Equals(e, EstadoDocuten.Error, StringComparison.OrdinalIgnoreCase)))
        {
            if (Estado != EstadoDocumento.Finalizado)
            {
                Estado = EstadoDocumento.Error;
            }

            return;
        }

        if (estados.Any(e => string.Equals(e, EstadoDocuten.Cancelled, StringComparison.OrdinalIgnoreCase)))
        {
            if (Estado != EstadoDocumento.Finalizado)
            {
                Estado = EstadoDocumento.Cancelado;
            }

            return;
        }

        var todosFinalizados = _envios.All(e =>
            string.Equals(e.PlataformaEnvioEstado, EstadoDocuten.Delivered, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.PlataformaEnvioEstado, EstadoDocuten.Completed, StringComparison.OrdinalIgnoreCase));

        if (todosFinalizados)
        {
            Estado = EstadoDocumento.Finalizado;
            return;
        }

        if (Estado is EstadoDocumento.Enviando or EstadoDocumento.Error)
        {
            Estado = EstadoDocumento.PendienteFirmas;
        }
    }

    private void AsegurarEstadoNuevo()
    {
        if (Estado != EstadoDocumento.Nuevo)
        {
            throw new InvalidOperationException(
                $"La operación no está permitida para un documento " +
                $"en estado '{Estado}'.");
        }
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
