using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Modules;

public sealed class UserPermissionsRepository : IUserPermissionsRepository
{
    private readonly AppDbContext _context;

    public UserPermissionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(
        Guid usuarioId,
        string moduleCode,
        ModulePermissionAction action,
        CancellationToken cancellationToken = default)
    {
        var permission = await _context.Set<Domain.Entities.PermissaoUsuario>()
            .AsNoTracking()
            .Include(p => p.Modulo)
            .FirstOrDefaultAsync(
                p => p.UsuarioId == usuarioId && p.Modulo.Codigo == moduleCode && p.Modulo.Ativo,
                cancellationToken);

        return permission?.Allows(action) ?? false;
    }
}
