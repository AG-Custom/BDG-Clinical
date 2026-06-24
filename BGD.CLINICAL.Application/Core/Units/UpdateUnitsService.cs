using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Core.Units;

public interface IUpdateUnitsService
{
    Task<Result<UnitDto>> ExecuteAsync(
        Guid id,
        UpdateUnitRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateUnitsService : IUpdateUnitsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitsService(
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
        Guid id,
        UpdateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<UnitDto>.Failure("Informe o nome da unidade.");
        }

        var empresaId = _tenantContext.EmpresaId;
        var unidade = await _unitsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (unidade is null)
        {
            return Result<UnitDto>.Failure("Unidade não encontrada.");
        }

        if (!unidade.Ativo)
        {
            return Result<UnitDto>.Failure("Não é possível editar uma unidade inativa.");
        }

        var nome = request.Nome.Trim();

        if (await _unitsRepository.ExistsByNomeAsync(empresaId, nome, id, cancellationToken))
        {
            return Result<UnitDto>.Failure("Já existe uma unidade com este nome.");
        }

        var dadosAnteriores = UnitsAuditSerializer.Serialize(unidade);
        var endereco = string.IsNullOrWhiteSpace(request.Endereco) ? null : request.Endereco.Trim();

        unidade.UpdateDetails(nome, endereco);
        _unitsRepository.Update(unidade);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Unidade),
            unidade.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: UnitsAuditSerializer.Serialize(unidade),
            cancellationToken: cancellationToken);

        return Result<UnitDto>.Success(UnitsMapper.Map(unidade));
    }
}
