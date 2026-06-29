using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Modules.Dtos;
using BGD.CLINICAL.Application.Modules.Permissions;

namespace BGD.CLINICAL.Application.Modules.EmployeePermissions;

public interface IUpdateEmployeePermissionsService
{
    Task<Result<EmployeePermissionsDto>> ExecuteAsync(
        Guid employeeId,
        UpdateEmployeePermissionsRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateEmployeePermissionsService : IUpdateEmployeePermissionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IUserPermissionsRepository _userPermissionsRepository;
    private readonly IUsersPermissionMutationService _mutationService;
    private readonly IGetEmployeePermissionsService _getEmployeePermissionsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeePermissionsService(
        ICurrentTenantContext tenantContext,
        IEmployeesRepository employeesRepository,
        IUserPermissionsRepository userPermissionsRepository,
        IUsersPermissionMutationService mutationService,
        IGetEmployeePermissionsService getEmployeePermissionsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _employeesRepository = employeesRepository;
        _userPermissionsRepository = userPermissionsRepository;
        _mutationService = mutationService;
        _getEmployeePermissionsService = getEmployeePermissionsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EmployeePermissionsDto>> ExecuteAsync(
        Guid employeeId,
        UpdateEmployeePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var funcionario = await _employeesRepository.GetByIdAndEmpresaIdAsync(
            employeeId,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (funcionario is null)
        {
            return Result<EmployeePermissionsDto>.Failure("Funcionário não encontrado.");
        }

        var usuarioId = await _employeesRepository.GetUsuarioIdByFuncionarioAndEmpresaAsync(
            employeeId,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (usuarioId is null)
        {
            return Result<EmployeePermissionsDto>.Failure("Funcionário não possui usuário de acesso.");
        }

        await _userPermissionsRepository.ReplaceOverridesAsync(
            usuarioId.Value,
            request.Allows,
            request.Denies,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _mutationService.InvalidateUsuarioAsync(usuarioId.Value, cancellationToken);

        return await _getEmployeePermissionsService.ExecuteAsync(employeeId, cancellationToken);
    }
}
