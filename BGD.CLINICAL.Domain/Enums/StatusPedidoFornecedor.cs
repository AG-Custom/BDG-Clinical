namespace BGD.CLINICAL.Domain.Enums;

public enum StatusPedidoFornecedor
{
    Pendente = 1,
    EnviadoParaFornecedor = 2,
    RecebidoPelaUnidade = 3,
    Cancelado = 4,
    Recusado = 5
}

public static class StatusPedidoFornecedorExtensions
{
    public const string PendenteApi = "Pendente";
    public const string EnviadoParaFornecedorApi = "Enviado para Fornecedor";
    public const string RecebidoPelaUnidadeApi = "Recebido pela Unidade";
    public const string CanceladoApi = "Cancelado";
    public const string RecusadoApi = "Recusado";

    public static string ToApiString(this StatusPedidoFornecedor status)
    {
        return status switch
        {
            StatusPedidoFornecedor.Pendente => PendenteApi,
            StatusPedidoFornecedor.EnviadoParaFornecedor => EnviadoParaFornecedorApi,
            StatusPedidoFornecedor.RecebidoPelaUnidade => RecebidoPelaUnidadeApi,
            StatusPedidoFornecedor.Cancelado => CanceladoApi,
            StatusPedidoFornecedor.Recusado => RecusadoApi,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static bool TryParseFromApi(string? value, out StatusPedidoFornecedor status)
    {
        status = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();

        foreach (StatusPedidoFornecedor candidate in Enum.GetValues<StatusPedidoFornecedor>())
        {
            if (string.Equals(candidate.ToApiString(), normalized, StringComparison.OrdinalIgnoreCase))
            {
                status = candidate;
                return true;
            }
        }

        if (string.Equals(normalized, "Aberto", StringComparison.OrdinalIgnoreCase))
        {
            status = StatusPedidoFornecedor.Pendente;
            return true;
        }

        if (string.Equals(normalized, "Pedido", StringComparison.OrdinalIgnoreCase))
        {
            status = StatusPedidoFornecedor.EnviadoParaFornecedor;
            return true;
        }

        if (string.Equals(normalized, "Recebido", StringComparison.OrdinalIgnoreCase))
        {
            status = StatusPedidoFornecedor.RecebidoPelaUnidade;
            return true;
        }

        return false;
    }

    public static StatusPedidoFornecedor FromStorage(string value)
    {
        if (TryParseFromApi(value, out var status))
        {
            return status;
        }

        throw new InvalidOperationException($"Status de pedido inválido: {value}");
    }

    public static bool IsEditable(StatusPedidoFornecedor status)
    {
        return status is StatusPedidoFornecedor.Pendente or StatusPedidoFornecedor.EnviadoParaFornecedor;
    }

    public static bool IsOpen(StatusPedidoFornecedor status)
    {
        return IsEditable(status);
    }

    public static string EditableStatusApiList =>
        $"{PendenteApi}, {EnviadoParaFornecedorApi}";

    public static string AllStatusApiList =>
        $"{PendenteApi}, {EnviadoParaFornecedorApi}, {RecebidoPelaUnidadeApi}, {CanceladoApi}, {RecusadoApi}";
}
