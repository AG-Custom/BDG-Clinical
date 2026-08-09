using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Applications.Abstractions;
using AG.CLINICAL.Application.Applications.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Applications.Procedures;

public interface IUpdateProceduresService
{
    Task<Result<ProcedureDto>> ExecuteAsync(
        Guid id,
        UpdateProcedureRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateProceduresService : IUpdateProceduresService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProceduresService(
        ICurrentTenantContext tenantContext,
        IProceduresRepository proceduresRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _proceduresRepository = proceduresRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProcedureDto>> ExecuteAsync(
        Guid id,
        UpdateProcedureRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var procedimento = await _proceduresRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (procedimento is null)
        {
            return Result<ProcedureDto>.Failure("Procedimento não encontrado.");
        }

        if (!procedimento.Ativo)
        {
            return Result<ProcedureDto>.Failure("Não é possível editar um procedimento inativo.");
        }

        var validation = await ProcedureRequestValidator.ValidateAsync(
            empresaId,
            request.Nome,
            request.ProdutoAplicadoId,
            request.Observacoes,
            request.Itens,
            excludeProcedureId: id,
            _proceduresRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<ProcedureDto>.Failure(validation.Error!);
        }

        try
        {
            var dadosAnteriores = ProceduresAuditSerializer.Serialize(procedimento);
            var data = validation.Value!;

            procedimento.UpdateDetails(
                data.Nome,
                data.ProdutoAplicadoId,
                data.Observacoes,
                data.Itens);

            _proceduresRepository.Update(procedimento);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _proceduresRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                procedimento.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Procedimento),
                procedimento.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: ProceduresAuditSerializer.Serialize(persisted ?? procedimento),
                cancellationToken: cancellationToken);

            return Result<ProcedureDto>.Success(ProceduresMapper.Map(persisted ?? procedimento));
        }
        catch (DomainException exception)
        {
            return Result<ProcedureDto>.Failure(exception.Message);
        }
    }
}
