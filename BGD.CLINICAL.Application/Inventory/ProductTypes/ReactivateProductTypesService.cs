using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.ProductTypes;

public interface IReactivateProductTypesService
{
    Task<Result<ProductTypeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ReactivateProductTypesService : IReactivateProductTypesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductTypesRepository _productTypesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateProductTypesService(
        ICurrentTenantContext tenantContext,
        IProductTypesRepository productTypesRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _productTypesRepository = productTypesRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductTypeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var tipoProduto = await _productTypesRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (tipoProduto is null)
        {
            return Result<ProductTypeDto>.Failure("Tipo de produto não encontrado.");
        }

        if (tipoProduto.Ativo)
        {
            return Result<ProductTypeDto>.Failure("Tipo de produto já está ativo.");
        }

        if (await _productTypesRepository.ExistsByNomeAsync(empresaId, tipoProduto.Nome, tipoProduto.Id, cancellationToken))
        {
            return Result<ProductTypeDto>.Failure("Já existe um tipo de produto com este nome.");
        }

        var dadosAnteriores = ProductTypesAuditSerializer.Serialize(tipoProduto);

        tipoProduto.Reactivate();
        _productTypesRepository.Update(tipoProduto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(TipoProduto),
            tipoProduto.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: ProductTypesAuditSerializer.Serialize(tipoProduto),
            cancellationToken: cancellationToken);

        return Result<ProductTypeDto>.Success(ProductTypesMapper.Map(tipoProduto));
    }
}
