using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Entities;
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

    public async Task EnsureActiveLicenseAsync(
        Guid empresaId,
        Guid moduloId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.LicencasModulo
            .FirstOrDefaultAsync(
                licenca => licenca.EmpresaId == empresaId && licenca.ModuloId == moduloId,
                cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var licenca = new LicencaModulo(
            empresaId,
            moduloId,
            StatusLicencaModulo.Ativo,
            DateTime.UtcNow,
            dataFim: null,
            valor: 0);

        await _context.LicencasModulo.AddAsync(licenca, cancellationToken);
    }
}
