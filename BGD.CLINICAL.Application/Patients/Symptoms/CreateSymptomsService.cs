using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Patients.Symptoms;

public interface ICreateSymptomsService
{
    Task<Result<SymptomDto>> ExecuteAsync(
        CreateSymptomRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateSymptomsService : ICreateSymptomsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISymptomsRepository _symptomsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSymptomsService(
        ICurrentTenantContext tenantContext,
        ISymptomsRepository symptomsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _symptomsRepository = symptomsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SymptomDto>> ExecuteAsync(
        CreateSymptomRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<SymptomDto>.Failure("Informe o nome do sintoma.");
        }

        var nome = request.Nome.Trim();
        var empresaId = _tenantContext.EmpresaId;

        if (await _symptomsRepository.ExistsByNomeAsync(empresaId, nome, null, cancellationToken))
        {
            return Result<SymptomDto>.Failure("Já existe um sintoma com este nome.");
        }

        try
        {
            var sintoma = Sintoma.Create(empresaId, nome);

            await _symptomsRepository.AddAsync(sintoma, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Sintoma),
                sintoma.Id,
                AcaoAuditoria.Criar,
                dadosNovos: SymptomsAuditSerializer.Serialize(sintoma),
                cancellationToken: cancellationToken);

            return Result<SymptomDto>.Success(SymptomsMapper.Map(sintoma));
        }
        catch (DomainException exception)
        {
            return Result<SymptomDto>.Failure(exception.Message);
        }
    }
}
