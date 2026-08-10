using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Identity;

public sealed class FirstAccessInvitationsRepository : IFirstAccessInvitationsRepository
{
    private readonly AppDbContext _context;

    public FirstAccessInvitationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ConvitePrimeiroAcesso?> GetValidByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _context.ConvitesPrimeiroAcesso
            .Include(convite => convite.Usuario)
                .ThenInclude(usuario => usuario.Empresa)
            .FirstOrDefaultAsync(
                convite =>
                    convite.TokenHash == tokenHash
                    && convite.UtilizadoEm == null
                    && convite.ExpiraEm > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task InvalidatePendingByUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var pending = await _context.ConvitesPrimeiroAcesso
            .Where(convite => convite.UsuarioId == usuarioId && convite.UtilizadoEm == null)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        _context.ConvitesPrimeiroAcesso.RemoveRange(pending);
    }

    public async Task AddAsync(ConvitePrimeiroAcesso convite, CancellationToken cancellationToken = default)
    {
        await _context.ConvitesPrimeiroAcesso.AddAsync(convite, cancellationToken);
    }

    public void Update(ConvitePrimeiroAcesso convite)
    {
        _context.ConvitesPrimeiroAcesso.Update(convite);
    }
}
