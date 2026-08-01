using AG.CLINICAL.Application.Identity.Abstractions;

namespace AG.CLINICAL.WebApi.Infrastructure.Auth;

public sealed class BcryptPasswordHashGenerator : IPasswordHashGenerator
{
    public string GenerateHash(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha);
    }

    public bool Verify(string senha, string senhaHash)
    {
        return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
    }
}
