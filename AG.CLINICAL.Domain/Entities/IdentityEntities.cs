using AG.CLINICAL.Domain.Common;
using AG.CLINICAL.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace AG.CLINICAL.Domain.Entities;

public sealed class ConvitePrimeiroAcesso : Entity
{
    private ConvitePrimeiroAcesso()
    {
    }

    private ConvitePrimeiroAcesso(Guid usuarioId, string tokenHash, DateTime expiraEm)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        ExpiraEm = expiraEm;
    }

    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiraEm { get; private set; }
    public DateTime? UtilizadoEm { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public bool IsValid => UtilizadoEm is null && ExpiraEm > DateTime.UtcNow;

    public static (ConvitePrimeiroAcesso Convite, string RawToken) Create(Guid usuarioId, int expirationHours)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new DomainException("Usuário do convite não informado.");
        }

        if (expirationHours <= 0)
        {
            throw new DomainException("A validade do convite deve ser maior que zero.");
        }

        var rawToken = GenerateRawToken();
        var convite = new ConvitePrimeiroAcesso(usuarioId, HashToken(rawToken), DateTime.UtcNow.AddHours(expirationHours));
        return (convite, rawToken);
    }

    public void MarkAsUsed()
    {
        if (UtilizadoEm.HasValue)
        {
            throw new DomainException("Convite já utilizado.");
        }

        if (ExpiraEm <= DateTime.UtcNow)
        {
            throw new DomainException("Convite expirado.");
        }

        UtilizadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
