using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ModificarAgencia;

internal sealed class ModificarAgenciaCommandHandler
    : IRequestHandler<
        ModificarAgenciaCommand,
        ErrorOr<AgenciaDto>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<AgenciaDto>> Handle(
        ModificarAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            return Error.Validation(
                "Agencia.DatosInvalidos",
                "El código de agencia es obligatorio.");
        }

        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var codigo = request.Codigo.Trim();

        if (!string.Equals(
                agencia.Codigo,
                codigo,
                StringComparison.OrdinalIgnoreCase))
        {
            var existente = await _agenciaRepository.GetByCodigoAsync(
                codigo,
                cancellationToken);

            if (existente is not null &&
                existente.Id != agencia.Id)
            {
                return Error.Conflict(
                    "Agencia.CodigoDuplicado",
                    $"Ya existe una agencia con el código '{codigo}'.");
            }
        }

        try
        {
            agencia.Modificar(
                codigo: codigo,
                nombre: request.Nombre,
                identificadorFiscal: request.IdentificadorFiscal,
                agenciaQs: request.AgenciaQs,
                entregaEnDestino: request.EntregaEnDestino,
                requierePrecinto: request.RequierePrecinto,
                direccion: request.Direccion,
                codigoPostal: request.CodigoPostal,
                municipio: request.Municipio,
                codigoPaisIso: request.CodigoPaisIso,
                requiereDatosPorteador: request.RequiereDatosPorteador);

            if (request.Activa)
            {
                agencia.Activar();
            }
            else
            {
                agencia.Desactivar();
            }
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "Agencia.DatosInvalidos",
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AgenciaDto(
            agencia.Id,
            agencia.Codigo,
            agencia.Nombre,
            agencia.IdentificadorFiscal,
            agencia.Activa,
            agencia.AgenciaQs,
            agencia.EntregaEnDestino,
            agencia.RequierePrecinto,
            agencia.Direccion,
            agencia.CodigoPostal,
            agencia.Municipio,
            agencia.CodigoPaisIso,
            agencia.RequiereDatosPorteador);
    }
}