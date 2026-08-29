using System.Text.Json;

namespace AG.CLINICAL.Application.Packages.PatientPurchases;

internal sealed record ProdutoSaldoSnapshot(
    Guid ProdutoId,
    string? ProdutoNome,
    string? UnidadeMedida,
    decimal QuantidadeContratada,
    decimal QuantidadeUtilizada);

internal static class PatientPurchaseSaldoAuditParser
{
    public static string? ExtrairMotivo(string? dadosNovos)
    {
        if (string.IsNullOrWhiteSpace(dadosNovos))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(dadosNovos);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (document.RootElement.TryGetProperty("motivo", out var motivoElement)
                || document.RootElement.TryGetProperty("Motivo", out motivoElement))
            {
                return motivoElement.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    public static Dictionary<Guid, ProdutoSaldoSnapshot> ExtrairSaldoProdutos(string? json)
    {
        var resultado = new Dictionary<Guid, ProdutoSaldoSnapshot>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return resultado;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            JsonElement saldoElement;
            if (root.TryGetProperty("saldo", out var saldoWrapper)
                || root.TryGetProperty("Saldo", out saldoWrapper))
            {
                if (saldoWrapper.TryGetProperty("Saldo", out var nested)
                    || saldoWrapper.TryGetProperty("saldo", out nested))
                {
                    saldoElement = nested;
                }
                else if (saldoWrapper.TryGetProperty("Produtos", out _)
                    || saldoWrapper.TryGetProperty("produtos", out _))
                {
                    saldoElement = saldoWrapper;
                }
                else
                {
                    return resultado;
                }
            }
            else if (root.TryGetProperty("Saldo", out var saldoDireto)
                || root.TryGetProperty("saldo", out saldoDireto))
            {
                saldoElement = saldoDireto;
            }
            else
            {
                return resultado;
            }

            if (!TryGetProperty(saldoElement, "Produtos", "produtos", out var produtosElement)
                || produtosElement.ValueKind != JsonValueKind.Array)
            {
                return resultado;
            }

            foreach (var produto in produtosElement.EnumerateArray())
            {
                if (!TryGetGuid(produto, "ProdutoId", "produtoId", out var produtoId))
                {
                    continue;
                }

                TryGetString(produto, "ProdutoNome", "produtoNome", out var produtoNome);
                TryGetString(produto, "UnidadeMedida", "unidadeMedida", out var unidadeMedida);
                TryGetDecimal(produto, "QuantidadeContratada", "quantidadeContratada", out var contratada);
                TryGetDecimal(produto, "QuantidadeUtilizada", "quantidadeUtilizada", out var utilizada);

                resultado[produtoId] = new ProdutoSaldoSnapshot(
                    produtoId,
                    produtoNome,
                    unidadeMedida,
                    contratada,
                    utilizada);
            }
        }
        catch (JsonException)
        {
            return resultado;
        }

        return resultado;
    }

    private static bool TryGetProperty(
        JsonElement element,
        string pascal,
        string camel,
        out JsonElement value)
    {
        if (element.TryGetProperty(pascal, out value) || element.TryGetProperty(camel, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetGuid(JsonElement element, string pascal, string camel, out Guid value)
    {
        value = Guid.Empty;
        if (!TryGetProperty(element, pascal, camel, out var property))
        {
            return false;
        }

        return property.TryGetGuid(out value)
            || (property.ValueKind == JsonValueKind.String
                && Guid.TryParse(property.GetString(), out value));
    }

    private static bool TryGetDecimal(
        JsonElement element,
        string pascal,
        string camel,
        out decimal value)
    {
        value = 0;
        if (!TryGetProperty(element, pascal, camel, out var property))
        {
            return false;
        }

        return property.TryGetDecimal(out value);
    }

    private static bool TryGetString(
        JsonElement element,
        string pascal,
        string camel,
        out string? value)
    {
        value = null;
        if (!TryGetProperty(element, pascal, camel, out var property))
        {
            return false;
        }

        value = property.GetString();
        return true;
    }
}
