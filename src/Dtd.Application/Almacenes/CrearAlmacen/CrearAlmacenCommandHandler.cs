using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.CrearAlmacen;

internal sealed class CrearAlmacenCommandHandler
    : IRequestHandler<
        CrearAlmacenCommand,
        ErrorOr<AlmacenDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearAlmacenCommandHandler(
        IAlmacenRepository almacenRepository,
        IUnitOfWork unitOfWork)
    {
        _almacenRepository = almacenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<AlmacenDto>> Handle(
        CrearAlmacenCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();
        var codigo = request.Codigo.Trim();

        var existente =
            await _almacenRepository.GetByEmpresaYCodigoAsync(
                empresa,
                codigo,
                cancellationToken);

        if (existente is not null)
        {
            return Error.Conflict(
                "Almacen.CodigoDuplicado",
                $"Ya existe un almacén con código '{codigo}' para la empresa '{empresa}'.");
        }

        var almacen = Almacen.Crear(
            empresa,
            codigo,
            request.Nombre,
            request.Direccion,
            request.CodigoPostal,
            request.Ciudad,
            request.CodigoPaisIso,
            request.Email,
            request.Telefono);

        almacen.ConfigurarTipoFirmaConsignor(
            request.TipoFirmaConsignor);

        await _almacenRepository.AddAsync(
            almacen,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return ToDto(almacen);
    }

    private static AlmacenDto ToDto(
        Almacen almacen)
    {
        return new AlmacenDto(
            almacen.Id,
            almacen.Codigo,
            almacen.Nombre,
            almacen.Direccion,
            almacen.CodigoPostal,
            almacen.Ciudad,
            almacen.CodigoPaisIso,
            almacen.Email?.Valor,
            almacen.Telefono);
    }
}