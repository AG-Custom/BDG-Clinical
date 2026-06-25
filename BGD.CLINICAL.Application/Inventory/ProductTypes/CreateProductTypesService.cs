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

public interface ICreateProductTypesService
{
    Task<Result<ProductTypeDto>> ExecuteAsync(
        CreateProductTypeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateProductTypesService : ICreateProductTypesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductTypesRepository _productTypesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductTypesService(
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
        CreateProductTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<ProductTypeDto>.Failure("Informe o nome do tipo de produto.");
        }

        var nome = request.Nome.Trim();
        var empresaId = _tenantContext.EmpresaId;

        if (await _productTypesRepository.ExistsByNomeAsync(empresaId, nome, null, cancellationToken))
        {
            return Result<ProductTypeDto>.Failure("Já existe um tipo de produto com este nome.");
        }

        try
        {
            var tipoProduto = TipoProduto.Create(empresaId, nome);

            await _productTypesRepository.AddAsync(tipoProduto, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(TipoProduto),
                tipoProduto.Id,
                AcaoAuditoria.Criar,
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
