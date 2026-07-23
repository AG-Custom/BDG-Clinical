using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.ProductTypes;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Infra.Data.Services.Inventory;

public sealed class CompanyDefaultProductTypesProvisioner : ICompanyDefaultProductTypesProvisioner
{
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IProductTypesRepository _productTypesRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyDefaultProductTypesProvisioner(
        IRepository<Empresa> empresaRepository,
        IProductTypesRepository productTypesRepository,
        IUnitOfWork unitOfWork)
    {
        _empresaRepository = empresaRepository;
        _productTypesRepository = productTypesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ProvisionDefaultProductTypesAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        foreach (var template in DefaultProductTypesCatalog.All)
        {
            if (await _productTypesRepository.ExistsByCodigoAsync(
                    empresaId,
                    template.Codigo,
                    cancellationToken))
            {
                continue;
            }

            var candidatos = await _productTypesRepository.ListByExactNomeWithoutCodigoAsync(
                empresaId,
                template.Nome,
                cancellationToken);

            if (candidatos.Count == 1)
            {
                candidatos[0].AssignSystemCodigo(template.Codigo);
                _productTypesRepository.Update(candidatos[0]);
                continue;
            }

            if (await _productTypesRepository.ExistsByNomeAsync(
                    empresaId,
                    template.Nome,
                    excludeId: null,
                    cancellationToken))
            {
                continue;
            }

            var tipoProduto = TipoProduto.Create(empresaId, template.Nome, template.Codigo);
            await _productTypesRepository.AddAsync(tipoProduto, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task BackfillAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _empresaRepository.ListAsync(cancellationToken);

        foreach (var empresa in empresas)
        {
            await ProvisionDefaultProductTypesAsync(empresa.Id, cancellationToken);
        }
    }
}
