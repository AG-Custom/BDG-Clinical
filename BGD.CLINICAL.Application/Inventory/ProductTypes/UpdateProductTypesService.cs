using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.ProductTypes;

public interface IUpdateProductTypesService
{
    Task<Result<ProductTypeDto>> ExecuteAsync(
        Guid id,
        UpdateProductTypeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateProductTypesService : IUpdateProductTypesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductTypesRepository _productTypesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductTypesService(
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
        UpdateProductTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<ProductTypeDto>.Failure("Informe o nome do tipo de produto.");
        }

        var empresaId = _tenantContext.EmpresaId;
        var tipoProduto = await _productTypesRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (tipoProduto is null)
        {
            return Result<ProductTypeDto>.Failure("Tipo de produto não encontrado.");
        }

        if (!tipoProduto.Ativo)
        {
            return Result<ProductTypeDto>.Failure("Não é possível editar um tipo de produto inativo.");
        }

        var nome = request.Nome.Trim();

        if (await _productTypesRepository.ExistsByNomeAsync(empresaId, nome, id, cancellationToken))
        {
            return Result<ProductTypeDto>.Failure("Já existe um tipo de produto com este nome.");
        }

        try
        {
            var dadosAnteriores = ProductTypesAuditSerializer.Serialize(tipoProduto);

            tipoProduto.UpdateDetails(nome);
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
        catch (DomainException exception)
        {
            return Result<ProductTypeDto>.Failure(exception.Message);
        }
    }
}
