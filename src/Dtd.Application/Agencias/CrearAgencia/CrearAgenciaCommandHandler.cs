using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.CrearAgencia;

internal sealed class CrearAgenciaCommandHandler
    : IRequestHandler<
        CrearAgenciaCommand,
        ErrorOr<AgenciaDto>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<AgenciaDto>> Handle(
        CrearAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var existente = await _agenciaRepository.GetByCodigoAsync(
            request.Codigo.Trim(),
            cancellationToken);

        if (existente is not null)
        {
            return Error.Conflict(
                "Agencia.CodigoDuplicado",
                $"Ya existe una agencia con el código '{request.Codigo}'.");
        }

        Agencia agencia;

        try
        {
            agencia = Agencia.Crear(
                request.Codigo,
                request.Nombre,
                request.IdentificadorFiscal,
                request.AgenciaQs,
                request.EntregaEnDestino,
                request.RequierePrecinto,
                request.Direccion,
                request.CodigoPostal,
                request.Municipio,
                request.CodigoPaisIso);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "Agencia.DatosInvalidos",
                ex.Message);
        }

        await _agenciaRepository.AddAsync(
            agencia,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

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
            agencia.CodigoPaisIso);
    }
}