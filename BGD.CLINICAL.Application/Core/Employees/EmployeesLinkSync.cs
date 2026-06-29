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
        IEmployeesRepository employeesRepository,
        CancellationToken cancellationToken)
    {
        if (linkToEmpresa)
        {
            SyncEmpresaVinculo(funcionario, empresaId, cargoId);
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

        SyncUnidadeVinculos(funcionario, empresaId, distinctUnidadeIds, cargoId);
        return Result.Success();
    }

    private static void SyncEmpresaVinculo(
        Funcionario funcionario,
        Guid empresaId,
        Guid? cargoId)
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

            empresaVinculo.UpdateAssignment(cargoId);
            return;
        }

        funcionario.AddEmpresaVinculo(empresaId, cargoId);
    }

    private static void SyncUnidadeVinculos(
        Funcionario funcionario,
        Guid empresaId,
        IReadOnlyList<Guid> unidadeIds,
        Guid? cargoId)
    {
        var requestedUnidadeIds = unidadeIds.ToHashSet();
        var knownUnidadeIdsInEmpresa = BuildKnownUnidadeIdsInEmpresa(funcionario, empresaId, requestedUnidadeIds);
        var vinculosNaEmpresa = GetVinculosInEmpresa(funcionario, empresaId, knownUnidadeIdsInEmpresa);

        foreach (var vinculo in vinculosNaEmpresa.Where(v => v.EmpresaId.HasValue && v.Ativo))
        {
            vinculo.Deactivate();
        }

        foreach (var unidadeId in unidadeIds)
        {
            var existingVinculo = funcionario.Vinculos.FirstOrDefault(v => v.UnidadeId == unidadeId);

            if (existingVinculo is not null)
            {
                if (!existingVinculo.Ativo)
                {
                    existingVinculo.Reactivate();
                }

                existingVinculo.UpdateAssignment(cargoId);
                continue;
            }

            funcionario.AddUnidadeVinculo(unidadeId, cargoId);
        }

        foreach (var vinculo in funcionario.Vinculos.Where(v =>
                     v.UnidadeId.HasValue
                     && v.Ativo
                     && !requestedUnidadeIds.Contains(v.UnidadeId.Value)
                     && BelongsToEmpresa(v, empresaId, knownUnidadeIdsInEmpresa)))
        {
            vinculo.Deactivate();
        }
    }

    private static HashSet<Guid> BuildKnownUnidadeIdsInEmpresa(
        Funcionario funcionario,
        Guid empresaId,
        IReadOnlySet<Guid> requestedUnidadeIds)
    {
        var knownUnidadeIds = new HashSet<Guid>(requestedUnidadeIds);

        foreach (var vinculo in funcionario.Vinculos.Where(v => v.UnidadeId.HasValue))
        {
            if (vinculo.Unidade?.EmpresaId == empresaId)
            {
                knownUnidadeIds.Add(vinculo.UnidadeId!.Value);
            }
        }

        return knownUnidadeIds;
    }

    private static List<FuncionarioVinculo> GetVinculosInEmpresa(
        Funcionario funcionario,
        Guid empresaId,
        IReadOnlySet<Guid>? knownUnidadeIdsInEmpresa = null)
    {
        return funcionario.Vinculos
            .Where(vinculo => BelongsToEmpresa(vinculo, empresaId, knownUnidadeIdsInEmpresa))
            .ToList();
    }

    private static bool BelongsToEmpresa(
        FuncionarioVinculo vinculo,
        Guid empresaId,
        IReadOnlySet<Guid>? knownUnidadeIdsInEmpresa = null)
    {
        if (vinculo.EmpresaId == empresaId)
        {
            return true;
        }

        if (!vinculo.UnidadeId.HasValue)
        {
            return false;
        }

        if (vinculo.Unidade?.EmpresaId == empresaId)
        {
            return true;
        }

        return knownUnidadeIdsInEmpresa?.Contains(vinculo.UnidadeId.Value) == true;
    }
}
