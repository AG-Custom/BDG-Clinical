using BGD.CLINICAL.Application.Abstractions.Identity;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Abstractions;

public interface IEmployeesRepository
{
    Task<IReadOnlyList<Funcionario>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<Funcionario?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> AllUnidadesBelongToEmpresaAsync(
        IReadOnlyList<Guid> unidadeIds,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<EmployeeUserAccessInfo?> GetUserAccessInfoByFuncionarioAndEmpresaAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Funcionario funcionario, CancellationToken cancellationToken = default);

    void Update(Funcionario funcionario);
}
