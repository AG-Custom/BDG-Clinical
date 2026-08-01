using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Identity.Abstractions;

public interface IFirstAccessInvitationsRepository
{
    Task<ConvitePrimeiroAcesso?> GetValidByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task InvalidatePendingByUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task AddAsync(ConvitePrimeiroAcesso convite, CancellationToken cancellationToken = default);

    void Update(ConvitePrimeiroAcesso convite);
}
