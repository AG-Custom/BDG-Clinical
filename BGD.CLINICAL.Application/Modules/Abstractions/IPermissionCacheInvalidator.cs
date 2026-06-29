namespace BGD.CLINICAL.Application.Modules.Abstractions;

public interface IPermissionCacheInvalidator
{
    void InvalidateUsuario(Guid usuarioId);

    void InvalidateUsuarios(IEnumerable<Guid> usuarioIds);
}
