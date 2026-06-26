using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Applications.Procedures;

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
