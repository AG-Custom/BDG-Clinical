namespace BGD.CLINICAL.Application.Abstractions.Security;

public interface ICurrentTenantContext
{
    Guid EmpresaId { get; }

    Guid UsuarioId { get; }
}
