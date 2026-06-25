using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Core.Positions;

public interface IUpdatePositionsService
{
    Task<Result<PositionDto>> ExecuteAsync(
        Guid id,
        UpdatePositionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdatePositionsService : IUpdatePositionsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePositionsService(
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
        Guid id,
        UpdatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<PositionDto>.Failure("Informe o nome do cargo.");
        }

        var empresaId = _tenantContext.EmpresaId;
        var cargo = await _positionsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (cargo is null)
        {
            return Result<PositionDto>.Failure("Cargo não encontrado.");
        }

        if (!cargo.Ativo)
        {
            return Result<PositionDto>.Failure("Não é possível editar um cargo inativo.");
        }

        var nome = request.Nome.Trim();

        if (await _positionsRepository.ExistsByNomeAsync(empresaId, nome, id, cancellationToken))
        {
            return Result<PositionDto>.Failure("Já existe um cargo com este nome.");
        }

        try
        {
            var dadosAnteriores = PositionsAuditSerializer.Serialize(cargo);

            cargo.UpdateDetails(nome);
            _positionsRepository.Update(cargo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Cargo),
                cargo.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: PositionsAuditSerializer.Serialize(cargo),
                cancellationToken: cancellationToken);

            return Result<PositionDto>.Success(PositionsMapper.Map(cargo));
        }
        catch (DomainException exception)
        {
            return Result<PositionDto>.Failure(exception.Message);
        }
    }
}
