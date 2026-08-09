using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Abstractions.Identity;

public interface IProvisionEmployeeUsersService
{
    Task<Usuario> ProvisionAsync(
        Guid empresaId,
        Guid funcionarioId,
        string nome,
        string emailLogin,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task DeactivateByFuncionarioAsync(
        Guid empresaId,
        Guid funcionarioId,
        CancellationToken cancellationToken = default);

    Task ReactivateByFuncionarioAsync(
        Guid empresaId,
        Guid funcionarioId,
        CancellationToken cancellationToken = default);
}
