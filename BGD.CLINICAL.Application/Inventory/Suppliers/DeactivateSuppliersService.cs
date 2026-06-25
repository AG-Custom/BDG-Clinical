using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.Suppliers;

public interface IDeactivateSuppliersService
{
    Task<Result<SupplierDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivateSuppliersService : IDeactivateSuppliersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISuppliersRepository _suppliersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSuppliersService(
        ICurrentTenantContext tenantContext,
        ISuppliersRepository suppliersRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _suppliersRepository = suppliersRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SupplierDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var fornecedor = await _suppliersRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (fornecedor is null)
        {
            return Result<SupplierDto>.Failure("Fornecedor não encontrado.");
        }

        if (!fornecedor.Ativo)
        {
            return Result<SupplierDto>.Failure("Fornecedor já está inativo.");
        }

        if (await _suppliersRepository.HasOpenPedidosAsync(id, empresaId, cancellationToken))
        {
            return Result<SupplierDto>.Failure(
                "Não é possível inativar um fornecedor com pedidos em aberto.");
        }

        var dadosAnteriores = SuppliersAuditSerializer.Serialize(fornecedor);

        fornecedor.Deactivate();
        _suppliersRepository.Update(fornecedor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Fornecedor),
            fornecedor.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: SuppliersAuditSerializer.Serialize(fornecedor),
            cancellationToken: cancellationToken);

        return Result<SupplierDto>.Success(SuppliersMapper.Map(fornecedor));
    }
}
