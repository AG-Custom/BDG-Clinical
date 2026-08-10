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

public interface ICreateProceduresService
{
    Task<Result<ProcedureDto>> ExecuteAsync(
        CreateProcedureRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateProceduresService : ICreateProceduresService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProceduresService(
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
        CreateProcedureRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var validation = await ProcedureRequestValidator.ValidateAsync(
            empresaId,
            request.Nome,
            request.ProdutoAplicadoId,
            request.Observacoes,
            request.Itens,
            excludeProcedureId: null,
            _proceduresRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<ProcedureDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var procedimento = Procedimento.Create(
                empresaId,
                data.Nome,
                data.ProdutoAplicadoId,
                data.Observacoes,
                data.Itens);

            await _proceduresRepository.AddAsync(procedimento, cancellationToken);
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
                AcaoAuditoria.Criar,
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
