using Dtd.Application.Documentos;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using Mapster;

namespace Dtd.Application.Mapping;

public static class MappingRegister
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ConductorAsignado, ConductorDto>()
            .Map(d => d.Codigo, s => s.ConductorCodigo)
            .Map(d => d.Channel, s => s.Canal.Valor)
            .Map(d => d.Email, s => s.Email != null ? s.Email.Valor : null)
            .Map(d => d.Movil, s => s.Movil != null ? s.Movil.Valor : null);

        config.NewConfig<CcAsignado, CcDto>()
            .Map(d => d.Codigo, s => s.CcCodigo)
            .Map(d => d.Email, s => s.Email != null ? s.Email.Valor : null);

        config.NewConfig<OrigenDocumento, OrigenDto>();

        config.NewConfig<DestinoEnvio, DestinoEnvioDto>();

        config.NewConfig<Expedicion, ExpedicionDto>()
            .Map(d => d.Pais, s => s.Destino.Pais)
            .Map(d => d.Provincia, s => s.Destino.Provincia)
            .Map(d => d.CodigoPostal, s => s.Destino.CodigoPostal)
            .Map(d => d.Municipio, s => s.Destino.Municipio)
            .Map(d => d.AlmacenDestino, s => s.Destino.AlmacenDestino);
    }
}
