using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.Products;

public interface IReactivateProductsService
{
    Task<Result<ProductDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ReactivateProductsService : IReactivateProductsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductsRepository _productsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateProductsService(
        ICurrentTenantContext tenantContext,
        IProductsRepository productsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _productsRepository = productsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var produto = await _productsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (produto is null)
        {
            return Result<ProductDto>.Failure("Produto não encontrado.");
        }

        if (produto.Ativo)
        {
            return Result<ProductDto>.Failure("Produto já está ativo.");
        }

        if (!await _productsRepository.ExistsActiveTipoProdutoInEmpresaAsync(produto.TipoProdutoId, empresaId, cancellationToken))
        {
            return Result<ProductDto>.Failure("O tipo de produto vinculado está inativo.");
        }

        if (!await _productsRepository.ExistsActiveUnidadeMedidaInEmpresaAsync(produto.UnidadeMedidaId, empresaId, cancellationToken))
        {
            return Result<ProductDto>.Failure("A unidade de medida vinculada está inativa.");
        }

        if (await _productsRepository.ExistsByNomeAsync(empresaId, produto.Nome, produto.Id, cancellationToken))
        {
            return Result<ProductDto>.Failure("Já existe um produto com este nome.");
        }

        var dadosAnteriores = ProductsAuditSerializer.Serialize(produto);

        produto.Reactivate();
        _productsRepository.Update(produto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var persisted = await _productsRepository.GetByIdAndEmpresaIdAsync(produto.Id, empresaId, cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Produto),
            produto.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: ProductsAuditSerializer.Serialize(persisted ?? produto),
            cancellationToken: cancellationToken);

        return Result<ProductDto>.Success(ProductsMapper.Map(persisted ?? produto));
    }
}
