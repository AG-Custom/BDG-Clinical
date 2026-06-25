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

    public Task<Usuario?> GetByEmailLoginAndEmpresaIdAsync(
        string emailLogin,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Usuarios
            .Include(usuario => usuario.Empresa)
            .FirstOrDefaultAsync(
                usuario =>
                    usuario.EmailLogin == emailLogin
                    && usuario.EmpresaId == empresaId
                    && usuario.AuthProvider == IdentityConstants.AuthProviderLocal,
                cancellationToken);
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

    public Task<bool> ExistsActiveEmailLoginByEmpresaAsync(
        Guid empresaId,
        string emailLogin,
        CancellationToken cancellationToken = default)
    {
        return _context.Usuarios.AnyAsync(
            usuario =>
                usuario.EmpresaId == empresaId
                && usuario.EmailLogin == emailLogin
                && usuario.Ativo
                && usuario.AuthProvider == IdentityConstants.AuthProviderLocal,
            cancellationToken);
    }

    public Task<Usuario?> GetByFuncionarioIdAndEmpresaIdAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Usuarios.FirstOrDefaultAsync(
            usuario => usuario.FuncionarioId == funcionarioId && usuario.EmpresaId == empresaId,
            cancellationToken);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }

    public void Update(Usuario usuario)
    {
        var entry = _context.Entry(usuario);

        if (entry.State == EntityState.Detached)
        {
            _context.Usuarios.Update(usuario);
        }
    }
}
