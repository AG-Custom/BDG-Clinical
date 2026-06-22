using BGD.CLINICAL.Application.Identity;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Identity;

public sealed class UsersRepository : IUsersRepository
{
    private readonly AppDbContext _context;

    public UsersRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Usuarios
            .Include(usuario => usuario.Empresa)
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ListByEmailLoginAsync(
        string emailLogin,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(usuario => usuario.Empresa)
            .Where(usuario =>
                usuario.EmailLogin == emailLogin
                && usuario.AuthProvider == IdentityConstants.AuthProviderLocal)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveEmailLoginAsync(string emailLogin, CancellationToken cancellationToken = default)
    {
        return _context.Usuarios.AnyAsync(
            usuario =>
                usuario.EmailLogin == emailLogin
                && usuario.Ativo
                && usuario.AuthProvider == IdentityConstants.AuthProviderLocal,
            cancellationToken);
    }

    public Task<bool> ExistsActiveByEmailAsync(string emailLogin, CancellationToken cancellationToken = default)
    {
        return _context.Usuarios.AnyAsync(
            usuario => usuario.EmailLogin == emailLogin && usuario.Ativo,
            cancellationToken);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
