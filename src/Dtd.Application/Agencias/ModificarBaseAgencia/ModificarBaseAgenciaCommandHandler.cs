using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ModificarBaseAgencia;

internal sealed class ModificarBaseAgenciaCommandHandler
    : IRequestHandler<
        ModificarBaseAgenciaCommand,
        ErrorOr<AgenciaBaseDto>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarBaseAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<AgenciaBaseDto>> Handle(
        ModificarBaseAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var agencia =
            await _agenciaRepository.GetByIdConBasesAsync(
                request.AgenciaId,
                cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                "La agencia indicada no existe.");
        }

        Email? email = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                email = Email.Create(request.Email);
            }
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "AgenciaBase.DatosContactoInvalidos",
                ex.Message);
        }

        AgenciaBase baseAgencia;

        try
        {
            baseAgencia = agencia.ModificarBase(
                request.BaseId,
                request.Nombre,
                email,
                request.Language,
                request.Direccion,
                request.CodigoPostal,
                request.Municipio,
                request.CodigoPaisIso);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "AgenciaBase.DatosInvalidos",
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AgenciaBaseDto(
            baseAgencia.Id,
            baseAgencia.Codigo,
            baseAgencia.Nombre,
            baseAgencia.Direccion,
            baseAgencia.CodigoPostal,
            baseAgencia.Municipio,
            baseAgencia.CodigoPaisIso,
            baseAgencia.Email?.Valor,
            baseAgencia.Language,
            baseAgencia.Activo);
    }
}