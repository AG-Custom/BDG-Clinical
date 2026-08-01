using AG.CLINICAL.Application.Abstractions.Identity;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Core.Abstractions;

public interface IEmployeesRepository
{
    Task<IReadOnlyList<Funcionario>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? unidadeId,
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

    Task<Guid?> GetUsuarioIdByFuncionarioAndEmpresaAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Funcionario funcionario, CancellationToken cancellationToken = default);

    void Update(Funcionario funcionario);
}
