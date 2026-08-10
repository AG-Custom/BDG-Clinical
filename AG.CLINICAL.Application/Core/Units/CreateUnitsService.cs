using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Core.Units;

public interface ICreateUnitsService
{
    Task<Result<UnitDto>> ExecuteAsync(
        CreateUnitRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateUnitsService : ICreateUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUnitsService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UnitDto>> ExecuteAsync(
        CreateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<UnitDto>.Failure("Informe o nome da unidade.");
        }

        var nome = request.Nome.Trim();
        var empresaId = _tenantContext.EmpresaId;

        if (await _unitsRepository.ExistsByNomeAsync(empresaId, nome, null, cancellationToken))
        {
            return Result<UnitDto>.Failure("Já existe uma unidade com este nome.");
        }

        var endereco = string.IsNullOrWhiteSpace(request.Endereco) ? null : request.Endereco.Trim();
        var unidade = new Unidade(empresaId, nome, endereco);

        await _unitsRepository.AddAsync(unidade, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Unidade),
            unidade.Id,
            AcaoAuditoria.Criar,
            dadosNovos: UnitsAuditSerializer.Serialize(unidade),
            cancellationToken: cancellationToken);

        return Result<UnitDto>.Success(UnitsMapper.Map(unidade));
    }
}
