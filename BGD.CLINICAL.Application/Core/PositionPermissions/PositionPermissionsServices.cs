using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Modules.Dtos;
using BGD.CLINICAL.Application.Modules.Permissions;

namespace BGD.CLINICAL.Application.Core.PositionPermissions;

public interface IGetPositionPermissionsService
{
    Task<Result<PositionPermissionsDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class GetPositionPermissionsService : IGetPositionPermissionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPositionsRepository _positionsRepository;

    public GetPositionPermissionsService(
        ICurrentTenantContext tenantContext,
        IPositionsRepository positionsRepository)
    {
        _tenantContext = tenantContext;
        _positionsRepository = positionsRepository;
    }

    public async Task<Result<PositionPermissionsDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var cargo = await _positionsRepository.GetByIdWithPermissoesAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (cargo is null)
        {
            return Result<PositionPermissionsDto>.Failure("Cargo não encontrado.");
        }

        return Result<PositionPermissionsDto>.Success(new PositionPermissionsDto(
            cargo.Id,
            cargo.Nome,
            cargo.Permissoes
                .Select(item => item.PermissionKey)
                .OrderBy(key => key)
                .ToList()));
    }
}

public interface IUpdatePositionPermissionsService
{
    Task<Result<PositionPermissionsDto>> ExecuteAsync(
        Guid id,
        UpdatePositionPermissionsRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdatePositionPermissionsService : IUpdatePositionPermissionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IUserPermissionAssignmentsRepository _assignmentsRepository;
    private readonly IUsersPermissionMutationService _mutationService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePositionPermissionsService(
        ICurrentTenantContext tenantContext,
        IPositionsRepository positionsRepository,
        IUserPermissionAssignmentsRepository assignmentsRepository,
        IUsersPermissionMutationService mutationService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _positionsRepository = positionsRepository;
        _assignmentsRepository = assignmentsRepository;
        _mutationService = mutationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PositionPermissionsDto>> ExecuteAsync(
        Guid id,
        UpdatePositionPermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var cargo = await _positionsRepository.GetByIdWithPermissoesAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (cargo is null)
        {
            return Result<PositionPermissionsDto>.Failure("Cargo não encontrado.");
        }

        cargo.ReplacePermissionItems(request.PermissionKeys);
        _positionsRepository.Update(cargo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var usuarioIds = await _assignmentsRepository.ListUsuarioIdsByCargoIdAsync(
            _tenantContext.EmpresaId,
            cargo.Id,
            cancellationToken);

        await _mutationService.InvalidateUsuariosAsync(usuarioIds, cancellationToken);

        return Result<PositionPermissionsDto>.Success(new PositionPermissionsDto(
            cargo.Id,
            cargo.Nome,
            cargo.Permissoes
                .Select(item => item.PermissionKey)
                .OrderBy(key => key)
                .ToList()));
    }
}
