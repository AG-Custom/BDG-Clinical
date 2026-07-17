using System.Text.Json;
using BGD.CLINICAL.Application.Packages.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Packages.Packages;

internal static class PackagesMapper
{
    public static PackageDto Map(Pacote pacote)
    {
        return new PackageDto(
            pacote.Id,
            pacote.Nome,
            pacote.Descricao,
            pacote.Valor,
            pacote.Ativo,
            pacote.Itens
                .Select(item => new PackageItemDto(
                    item.Id,
                    item.ProdutoId,
                    item.Produto?.Nome ?? string.Empty,
                    item.QuantidadeTotal,
                    item.UnidadeMedida))
                .ToList(),
            pacote.CriadoEm,
            pacote.AtualizadoEm);
    }

    public static IReadOnlyList<PackageDto> Map(IReadOnlyList<Pacote> pacotes)
    {
        return pacotes.Select(Map).ToList();
    }
}

internal static class PackagesAuditSerializer
{
    public static string Serialize(Pacote pacote)
    {
        return JsonSerializer.Serialize(PackagesMapper.Map(pacote));
    }
}

internal static class PatientPurchasesMapper
{
    public static PatientPurchaseBalanceDto MapBalance(CompraPaciente compra)
    {
        var produtos = (compra.Pacote?.Itens ?? [])
            .Select(item =>
            {
                var utilizada = compra.GetQuantidadeUtilizada(item.ProdutoId);
                return new PatientPurchaseProductBalanceDto(
                    item.ProdutoId,
                    item.Produto?.Nome ?? string.Empty,
                    item.UnidadeMedida,
                    item.QuantidadeTotal,
                    utilizada,
                    Math.Max(0, item.QuantidadeTotal - utilizada));
            })
            .ToList();

        return new PatientPurchaseBalanceDto(compra.Id, produtos);
    }

    public static PatientPurchaseDto Map(CompraPaciente compra)
    {
        return new PatientPurchaseDto(
            compra.Id,
            compra.PacienteId,
            compra.Paciente?.Nome ?? string.Empty,
            compra.PacoteId,
            compra.Pacote?.Nome ?? string.Empty,
            compra.UnidadeId,
            compra.Unidade?.Nome ?? string.Empty,
            compra.DataCompra,
            compra.Status,
            compra.Observacao,
            MapBalance(compra),
            compra.CriadoEm,
            compra.AtualizadoEm);
    }

    public static IReadOnlyList<PatientPurchaseDto> Map(IReadOnlyList<CompraPaciente> compras)
    {
        return compras.Select(Map).ToList();
    }
}

internal static class PatientPurchasesAuditSerializer
{
    public static string Serialize(CompraPaciente compra)
    {
        return JsonSerializer.Serialize(PatientPurchasesMapper.Map(compra));
    }
}
