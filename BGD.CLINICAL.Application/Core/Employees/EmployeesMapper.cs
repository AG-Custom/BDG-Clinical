using BGD.CLINICAL.Application.Abstractions.Identity;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Employees;

internal static class EmployeesMapper
{
    public static EmployeeDto Map(
        Funcionario funcionario,
        Guid empresaId,
        EmployeeUserAccessInfo? userAccessInfo)
    {
        return new EmployeeDto(
            funcionario.Id,
            funcionario.Nome,
            funcionario.Telefone,
            funcionario.Email,
            userAccessInfo?.EmailLogin ?? string.Empty,
            userAccessInfo?.PendentePrimeiroAcesso ?? false,
            userAccessInfo?.IsAdmin ?? false,
            funcionario.Ativo,
            MapLinks(funcionario, empresaId),
            funcionario.CriadoEm,
            funcionario.AtualizadoEm);
    }

    public static IReadOnlyList<EmployeeDto> Map(
        IReadOnlyList<Funcionario> funcionarios,
        Guid empresaId,
        IReadOnlyDictionary<Guid, EmployeeUserAccessInfo?> userAccessInfoByFuncionarioId)
    {
        return funcionarios
            .Select(funcionario => Map(
                funcionario,
                empresaId,
                userAccessInfoByFuncionarioId.GetValueOrDefault(funcionario.Id)))
            .ToList();
    }

    private static IReadOnlyList<EmployeeLinkDto> MapLinks(Funcionario funcionario, Guid empresaId)
    {
        return funcionario.Vinculos
            .Where(vinculo => vinculo.BelongsToEmpresa(empresaId))
            .Select(vinculo => new EmployeeLinkDto(
                vinculo.Id,
                vinculo.EmpresaId,
                vinculo.UnidadeId,
                vinculo.CargoId,
                vinculo.Ativo))
            .ToList();
    }
}
