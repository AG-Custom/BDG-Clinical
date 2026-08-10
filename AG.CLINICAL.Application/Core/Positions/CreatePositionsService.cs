using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Core.Positions;

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

        var cargo = new Cargo(empresaId, nome, request.FlagAplicador);

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
