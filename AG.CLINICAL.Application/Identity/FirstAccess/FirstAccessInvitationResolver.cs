using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Identity.FirstAccess;

internal static class FirstAccessInvitationResolver
{
    public static async Task<Result<ConvitePrimeiroAcesso>> ResolveAsync(
        string rawToken,
        string email,
        IFirstAccessInvitationsRepository invitationsRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || string.IsNullOrWhiteSpace(email))
        {
            return Result<ConvitePrimeiroAcesso>.Failure(IdentityConstants.ConviteInvalido);
        }

        var tokenHash = ConvitePrimeiroAcesso.HashToken(rawToken.Trim());
        var convite = await invitationsRepository.GetValidByTokenHashAsync(tokenHash, cancellationToken);

        if (convite is null || !convite.IsValid)
        {
            return Result<ConvitePrimeiroAcesso>.Failure(IdentityConstants.ConviteInvalido);
        }

        var normalizedEmail = IdentityValidation.NormalizeEmail(email);
        var normalizedLogin = IdentityValidation.NormalizeEmail(convite.Usuario.EmailLogin);

        if (!string.Equals(normalizedEmail, normalizedLogin, StringComparison.Ordinal))
        {
            return Result<ConvitePrimeiroAcesso>.Failure(IdentityConstants.ConviteInvalido);
        }

        if (!convite.Usuario.PendentePrimeiroAcesso || !convite.Usuario.Ativo || !convite.Usuario.Empresa.Ativo)
        {
            return Result<ConvitePrimeiroAcesso>.Failure(IdentityConstants.ConviteInvalido);
        }

        return Result<ConvitePrimeiroAcesso>.Success(convite);
    }
}
