using System.Security.Claims;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Identity;

namespace BGD.CLINICAL.WebApi.Infrastructure.Security;

public sealed class CurrentTenantContext : ICurrentTenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid EmpresaId => GetRequiredGuidClaim(IdentityConstants.ClaimEmpresaId);

    public Guid UsuarioId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException("Usuário não autenticado.");

            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(value, out var id))
            {
                throw new InvalidOperationException("Claim de usuário inválida ou ausente.");
            }

            return id;
        }
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("Usuário não autenticado.");

        var value = user.FindFirst(claimType)?.Value;

        if (!Guid.TryParse(value, out var id))
        {
            throw new InvalidOperationException($"Claim '{claimType}' inválida ou ausente.");
        }

        return id;
    }
}
