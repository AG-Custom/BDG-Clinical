using AG.CLINICAL.Application.Abstractions.Identity;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;

namespace AG.CLINICAL.Application.Core.Employees;

public interface IListEmployeesService
{
    Task<Result<IReadOnlyList<EmployeeDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListEmployeesService : IListEmployeesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IEmployeesRepository _employeesRepository;

    public ListEmployeesService(
        ICurrentTenantContext tenantContext,
        IEmployeesRepository employeesRepository)
    {
        _tenantContext = tenantContext;
        _employeesRepository = employeesRepository;
    }

    public async Task<Result<IReadOnlyList<EmployeeDto>>> ExecuteAsync(
        Guid? unidadeId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var funcionarios = await _employeesRepository.ListByEmpresaIdAsync(
            empresaId,
            unidadeId,
            includeInactive,
            cancellationToken);

        var userAccessInfoByFuncionarioId = new Dictionary<Guid, EmployeeUserAccessInfo?>();

        foreach (var funcionario in funcionarios)
        {
            userAccessInfoByFuncionarioId[funcionario.Id] =
                await _employeesRepository.GetUserAccessInfoByFuncionarioAndEmpresaAsync(
                    funcionario.Id,
                    empresaId,
                    cancellationToken);
        }

        return Result<IReadOnlyList<EmployeeDto>>.Success(
            EmployeesMapper.Map(funcionarios, empresaId, userAccessInfoByFuncionarioId));
    }
}
