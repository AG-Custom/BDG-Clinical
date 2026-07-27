using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal sealed record ResolvedPatientApplicationProcedure(
    Guid ProcedimentoId,
    decimal? QuantidadeUtilizada);

internal static class PatientApplicationProcedureResolver
{
    public static Result<IReadOnlyList<ResolvedPatientApplicationProcedure>> Resolve(
        Guid? procedimentoId,
        decimal? quantidadeUtilizada,
        IReadOnlyList<CreatePatientApplicationProcedureRequest>? procedimentos)
    {
        IReadOnlyList<ResolvedPatientApplicationProcedure> resolved;

        if (procedimentos is { Count: > 0 })
        {
            resolved = procedimentos
                .Where(item => item.ProcedimentoId != Guid.Empty)
                .Select(item => new ResolvedPatientApplicationProcedure(
                    item.ProcedimentoId,
                    item.QuantidadeUtilizada))
                .ToList();
        }
        else if (procedimentoId.HasValue && procedimentoId.Value != Guid.Empty)
        {
            resolved =
            [
                new ResolvedPatientApplicationProcedure(procedimentoId.Value, quantidadeUtilizada)
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
