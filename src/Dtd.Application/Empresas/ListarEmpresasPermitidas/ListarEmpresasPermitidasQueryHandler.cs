using Dtd.Application.Common.Security;
using Dtd.Application.GatewayContracts;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Empresas.ListarEmpresasPermitidas
{
    internal sealed class ListarEmpresasPermitidasQueryHandler
        : IRequestHandler<
            ListarEmpresasPermitidasQuery,
            ErrorOr<IReadOnlyCollection<EmpresaDto>>>
    {
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IUserPermissionService _userPermissionService;

        public ListarEmpresasPermitidasQueryHandler(
            IEmpresaRepository empresaRepository,
            IUserPermissionService userPermissionService)
        {
            _empresaRepository = empresaRepository;
            _userPermissionService = userPermissionService;
        }

        public async Task<ErrorOr<IReadOnlyCollection<EmpresaDto>>> Handle(
            ListarEmpresasPermitidasQuery request,
            CancellationToken cancellationToken)
        {
            var empresas = await _empresaRepository.ListarAsync(
                cancellationToken);

            var codigosPermitidos =
                await _userPermissionService.GetAllowedCompaniesAsync(
                    cancellationToken);

            var permitidas = codigosPermitidos
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var resultado = empresas
                .Where(e => permitidas.Contains(e.Empresa))
                .OrderBy(e => e.Nombre)
                .Select(e => new EmpresaDto(
                    e.Empresa,
                    e.Nombre))
                .ToList();

            return resultado;
        }
    }
}