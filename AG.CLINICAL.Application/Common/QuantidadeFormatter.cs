using System.Globalization;

namespace AG.CLINICAL.Application.Common;

public static class QuantidadeFormatter
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Format(decimal value)
    {
        return value.ToString("0.####", PtBr);
    }
}
