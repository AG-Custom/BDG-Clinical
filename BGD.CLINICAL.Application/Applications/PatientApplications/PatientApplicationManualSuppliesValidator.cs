using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Domain.Constants;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal static class PatientApplicationManualSuppliesValidator
{
    public static Result<IReadOnlyList<PatientApplicationManualSupplyRequest>> NormalizeAndValidate(
        bool consumirInsumosKit,
        IReadOnlyList<PatientApplicationManualSupplyRequest>? insumosManuais,
        Guid? produtoAplicadoId,
        IReadOnlyDictionary<Guid, Produto> productsById)
    {
        var informados = (insumosManuais ?? [])
            .Where(item => item.ProdutoId != Guid.Empty)
            .ToList();

        if (consumirInsumosKit)
        {
            if (informados.Count > 0)
            {
                return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Failure(
                    "Insumos manuais não podem ser informados quando o consumo do kit está ativo.");
            }

            return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Success([]);
        }

        if (informados.Count == 0)
        {
            return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Success([]);
        }

        var ids = informados.Select(item => item.ProdutoId).ToList();
        if (ids.Distinct().Count() != ids.Count)
        {
            return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Failure(
                "Não é permitido repetir o mesmo insumo na lista manual.");
        }

        var normalizados = new List<PatientApplicationManualSupplyRequest>();

        foreach (var item in informados)
        {
            if (item.Quantidade <= 0)
            {
                return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Failure(
                    "A quantidade de cada insumo manual deve ser maior que zero.");
            }

            if (produtoAplicadoId.HasValue && item.ProdutoId == produtoAplicadoId.Value)
            {
                return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Failure(
                    "O produto aplicado não pode ser informado como insumo manual.");
            }

            if (!productsById.TryGetValue(item.ProdutoId, out var produto))
            {
                return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Failure(
                    "Um ou mais insumos manuais não foram encontrados ou estão inativos.");
            }

            if (produto.TipoProduto?.Codigo != ProductTypeCodes.Insumo)
            {
                return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Failure(
                    $"O produto \"{produto.Nome}\" não é um insumo.");
            }

            normalizados.Add(new PatientApplicationManualSupplyRequest(item.ProdutoId, item.Quantidade));
        }

        return Result<IReadOnlyList<PatientApplicationManualSupplyRequest>>.Success(normalizados);
    }
}
