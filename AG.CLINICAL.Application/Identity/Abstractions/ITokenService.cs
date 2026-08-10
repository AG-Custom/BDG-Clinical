using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Identity.Abstractions;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
}
