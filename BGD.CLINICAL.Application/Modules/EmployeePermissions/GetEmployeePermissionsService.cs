using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Modules.Dtos;

namespace BGD.CLINICAL.Application.Modules.EmployeePermissions;

public interface IGetEmployeePermissionsService
{
    Task<Result<EmployeePermissionsDto>> ExecuteAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

public sealed class GetEmployeePermissionsService : IGetEmployeePermissionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IUserPermissionsRepository _userPermissionsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IPermissionChecker _permissionChecker;

    public GetEmployeePermissionsService(
        ICurrentTenantContext tenantContext,
        IEmployeesRepository employeesRepository,
        IUserPermissionsRepository userPermissionsRepository,
        IPositionsRepository positionsRepository,
        IPermissionChecker permissionChecker)
    {
        _tenantContext = tenantContext;
        _employeesRepository = employeesRepository;
        _userPermissionsRepository = userPermissionsRepository;
        _positionsRepository = positionsRepository;
        _permissionChecker = permissionChecker;
    }

    public async Task<Result<EmployeePermissionsDto>> ExecuteAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var funcionario = await _employeesRepository.GetByIdAndEmpresaIdAsync(
            employeeId,
            empresaId,
            cancellationToken);

        if (funcionario is null)
        {
            return Result<EmployeePermissionsDto>.Failure("Funcionário não encontrado.");
        }

        var userAccess = await _employeesRepository.GetUserAccessInfoByFuncionarioAndEmpresaAsync(
            employeeId,
            empresaId,
            cancellationToken);

        if (userAccess is null)
        {
            return Result<EmployeePermissionsDto>.Success(new EmployeePermissionsDto(
                employeeId,
                null,
                null,
                null,
                [],
                [],
                [],
                []));
        }

        var usuarioId = await _employeesRepository.GetUsuarioIdByFuncionarioAndEmpresaAsync(
            employeeId,
            empresaId,
            cancellationToken);

        if (usuarioId is null)
        {
            return Result<EmployeePermissionsDto>.Failure("Usuário do funcionário não encontrado.");
        }

        var (cargoId, cargoNome, cargoPermissionKeys) = await ResolveCargoAsync(
            funcionario,
            empresaId,
            cancellationToken);

        var (allows, denies) = await _userPermissionsRepository.GetOverridesAsync(
            usuarioId.Value,
            cancellationToken);

        var effective = await _permissionChecker.GetEffectivePermissionsAsync(usuarioId.Value, cancellationToken);

        return Result<EmployeePermissionsDto>.Success(new EmployeePermissionsDto(
            employeeId,
            usuarioId,
            cargoId,
            cargoNome,
            cargoPermissionKeys,
            allows,
            denies,
            effective.OrderBy(key => key).ToList()));
    }

    private async Task<(Guid? CargoId, string? CargoNome, IReadOnlyList<string> PermissionKeys)> ResolveCargoAsync(
        Domain.Entities.Funcionario funcionario,
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        var activeVinculos = funcionario.Vinculos
            .Where(vinculo => vinculo.Ativo && vinculo.BelongsToEmpresa(empresaId))
            .ToList();

        var cargoIds = activeVinculos
            .Where(vinculo => vinculo.CargoId.HasValue)
            .Select(vinculo => vinculo.CargoId!.Value)
            .Distinct()
            .ToList();

        if (cargoIds.Count == 0)
        {
            return (null, null, []);
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? cargoNome = null;
        Guid? primaryCargoId = null;

        foreach (var cargoId in cargoIds)
        {
            var cargo = await _positionsRepository.GetByIdWithPermissoesAsync(
                cargoId,
                empresaId,
                cancellationToken);

            if (cargo is null || !cargo.Ativo)
            {
                continue;
            }

            primaryCargoId ??= cargo.Id;
            cargoNome ??= cargo.Nome;

            foreach (var item in cargo.Permissoes)
            {
                keys.Add(item.PermissionKey);
            }
        }

        return (primaryCargoId, cargoNome, keys.OrderBy(key => key).ToList());
    }
}
