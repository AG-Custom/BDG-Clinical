using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Core.Positions;

public interface ICreatePositionsService
{
    Task<Result<PositionDto>> ExecuteAsync(
        CreatePositionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreatePositionsService : ICreatePositionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePositionsService(
        ICurrentTenantContext tenantContext,
        IPositionsRepository positionsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _positionsRepository = positionsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PositionDto>> ExecuteAsync(
        CreatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<PositionDto>.Failure("Informe o nome do cargo.");
        }

        var nome = request.Nome.Trim();
        var empresaId = _tenantContext.EmpresaId;

        if (await _positionsRepository.ExistsByNomeAsync(empresaId, nome, null, cancellationToken))
        {
            return Result<PositionDto>.Failure("Já existe um cargo com este nome.");
        }

        var cargo = new Cargo(empresaId, nome);

        await _positionsRepository.AddAsync(cargo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Cargo),
            cargo.Id,
            AcaoAuditoria.Criar,
            dadosNovos: PositionsAuditSerializer.Serialize(cargo),
            cancellationToken: cancellationToken);

        return Result<PositionDto>.Success(PositionsMapper.Map(cargo));
    }
}
