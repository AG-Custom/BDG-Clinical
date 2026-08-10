using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Core.Abstractions;

public interface ICompaniesRepository
{
    Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCnpjAsync(
        string cnpj,
        Guid? excludeEmpresaId,
        CancellationToken cancellationToken = default);

    void Update(Empresa empresa);
}
