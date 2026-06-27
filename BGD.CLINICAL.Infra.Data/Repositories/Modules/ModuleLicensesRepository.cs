using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Modules;

public sealed class ModuleLicensesRepository : IModuleLicensesRepository
{
    private readonly AppDbContext _context;

    public ModuleLicensesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasActiveLicenseAsync(
        Guid empresaId,
        string moduleCode,
        CancellationToken cancellationToken = default)
    {
        var referenceUtc = DateTime.UtcNow;

        return await _context.LicencasModulo
            .AsNoTracking()
            .Include(licenca => licenca.Modulo)
            .AnyAsync(
                licenca =>
                    licenca.EmpresaId == empresaId
                    && licenca.Modulo.Codigo == moduleCode
                    && licenca.Modulo.Ativo
                    && (licenca.Status == StatusLicencaModulo.Ativo || licenca.Status == StatusLicencaModulo.Teste)
                    && licenca.DataInicio <= referenceUtc
                    && (licenca.DataFim == null || licenca.DataFim >= referenceUtc),
                cancellationToken);
    }
}
