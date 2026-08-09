using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Core;

public sealed class CompaniesRepository : ICompaniesRepository
{
    private readonly AppDbContext _context;

    public CompaniesRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Empresas.FirstOrDefaultAsync(empresa => empresa.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByCnpjAsync(
        string cnpj,
        Guid? excludeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var normalizedCnpj = cnpj.Trim();

        return _context.Empresas.AnyAsync(
            empresa => empresa.Cnpj == normalizedCnpj
                && (!excludeEmpresaId.HasValue || empresa.Id != excludeEmpresaId.Value),
            cancellationToken);
    }

    public void Update(Empresa empresa)
    {
        _context.Empresas.Update(empresa);
    }
}
