using AG.CLINICAL.Application.Applications.Dtos;
using AG.CLINICAL.Application.Common;

namespace AG.CLINICAL.Application.Applications.PatientApplications;

internal sealed record ResolvedPatientApplicationProcedure(
    Guid ProcedimentoId,
    decimal? QuantidadeUtilizada,
    Guid? LoteProdutoId,
    bool ConsumirInsumosKit,
    IReadOnlyList<PatientApplicationManualSupplyRequest>? InsumosManuais);

internal static class PatientApplicationProcedureResolver
{
    public static Result<IReadOnlyList<ResolvedPatientApplicationProcedure>> Resolve(
        Guid? procedimentoId,
        decimal? quantidadeUtilizada,
        Guid? loteProdutoId,
        bool consumirInsumosKit,
        IReadOnlyList<PatientApplicationManualSupplyRequest>? insumosManuais,
        IReadOnlyList<CreatePatientApplicationProcedureRequest>? procedimentos)
    {
        IReadOnlyList<ResolvedPatientApplicationProcedure> resolved;

        if (procedimentos is { Count: > 0 })
        {
            resolved = procedimentos
                .Where(item => item.ProcedimentoId != Guid.Empty)
                .Select(item => new ResolvedPatientApplicationProcedure(
                    item.ProcedimentoId,
                    item.QuantidadeUtilizada,
                    item.LoteProdutoId,
                    item.ConsumirInsumosKit,
                    item.InsumosManuais))
                .ToList();
        }
        else if (procedimentoId.HasValue && procedimentoId.Value != Guid.Empty)
        {
            resolved =
            [
                new ResolvedPatientApplicationProcedure(
                    procedimentoId.Value,
                    quantidadeUtilizada,
                    loteProdutoId,
                    consumirInsumosKit,
                    insumosManuais)
            ];
        }
        else
        {
            resolved = [];
        }

        if (resolved.Count == 0)
        {
            return Result<IReadOnlyList<ResolvedPatientApplicationProcedure>>.Failure(
                "Informe ao menos um procedimento.");
        }

        var ids = resolved.Select(item => item.ProcedimentoId).ToList();
        if (ids.Distinct().Count() != ids.Count)
        {
            return Result<IReadOnlyList<ResolvedPatientApplicationProcedure>>.Failure(
                "Não é permitido repetir o mesmo procedimento na aplicação.");
        }

        return Result<IReadOnlyList<ResolvedPatientApplicationProcedure>>.Success(resolved);
    }
}
