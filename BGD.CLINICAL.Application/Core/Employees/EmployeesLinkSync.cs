using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Employees;

internal static class EmployeesLinkSync
{
    public static async Task<Result> ApplyAsync(
        Funcionario funcionario,
        Guid empresaId,
        bool linkToEmpresa,
        IReadOnlyList<Guid>? unidadeIds,
        Guid? cargoId,
        bool flagAplicador,
        IEmployeesRepository employeesRepository,
        CancellationToken cancellationToken)
    {
        if (linkToEmpresa)
        {
            SyncEmpresaVinculo(funcionario, empresaId, cargoId, flagAplicador);
            return Result.Success();
        }

        if (unidadeIds is null || unidadeIds.Count == 0)
        {
            return Result.Failure("Informe ao menos uma unidade ou vincule o funcionário à empresa.");
        }

        var distinctUnidadeIds = unidadeIds.Distinct().ToList();

        if (!await employeesRepository.AllUnidadesBelongToEmpresaAsync(
                distinctUnidadeIds,
                empresaId,
                cancellationToken))
        {
            return Result.Failure("Uma ou mais unidades não pertencem à empresa.");
        }

        SyncUnidadeVinculos(funcionario, empresaId, distinctUnidadeIds, cargoId, flagAplicador);
        return Result.Success();
    }

    private static void SyncEmpresaVinculo(
        Funcionario funcionario,
        Guid empresaId,
        Guid? cargoId,
        bool flagAplicador)
    {
        var vinculosNaEmpresa = GetVinculosInEmpresa(funcionario, empresaId);

        foreach (var vinculo in vinculosNaEmpresa.Where(v => v.UnidadeId.HasValue && v.Ativo))
        {
            vinculo.Deactivate();
        }

        var empresaVinculo = vinculosNaEmpresa.FirstOrDefault(v => v.EmpresaId == empresaId);

        if (empresaVinculo is not null)
        {
            if (!empresaVinculo.Ativo)
            {
                empresaVinculo.Reactivate();
            }

            empresaVinculo.UpdateAssignment(cargoId, flagAplicador);
            return;
        }

        funcionario.AddEmpresaVinculo(empresaId, cargoId, flagAplicador);
    }

    private static void SyncUnidadeVinculos(
        Funcionario funcionario,
        Guid empresaId,
        IReadOnlyList<Guid> unidadeIds,
        Guid? cargoId,
        bool flagAplicador)
    {
        var vinculosNaEmpresa = GetVinculosInEmpresa(funcionario, empresaId);
        var requestedUnidadeIds = unidadeIds.ToHashSet();

        foreach (var vinculo in vinculosNaEmpresa.Where(v => v.EmpresaId.HasValue && v.Ativo))
        {
            vinculo.Deactivate();
        }

        foreach (var unidadeId in unidadeIds)
        {
            var existingVinculo = vinculosNaEmpresa.FirstOrDefault(v => v.UnidadeId == unidadeId);

            if (existingVinculo is not null)
            {
                if (!existingVinculo.Ativo)
                {
                    existingVinculo.Reactivate();
                }

                existingVinculo.UpdateAssignment(cargoId, flagAplicador);
                continue;
            }

            funcionario.AddUnidadeVinculo(unidadeId, cargoId, flagAplicador);
        }

        foreach (var vinculo in vinculosNaEmpresa.Where(v =>
                     v.UnidadeId.HasValue
                     && v.Ativo
                     && !requestedUnidadeIds.Contains(v.UnidadeId.Value)))
        {
            vinculo.Deactivate();
        }
    }

    private static List<FuncionarioVinculo> GetVinculosInEmpresa(Funcionario funcionario, Guid empresaId)
    {
        return funcionario.Vinculos
            .Where(vinculo => vinculo.BelongsToEmpresa(empresaId))
            .ToList();
    }
}
