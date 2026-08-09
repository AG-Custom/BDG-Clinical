namespace AG.CLINICAL.Application.Identity.Abstractions;

public interface IPasswordHashGenerator
{
    string GenerateHash(string senha);

    bool Verify(string senha, string senhaHash);
}
