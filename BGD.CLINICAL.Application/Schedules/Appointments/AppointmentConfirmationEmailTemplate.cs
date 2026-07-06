using System.Globalization;
using System.Net;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

internal static class AppointmentConfirmationEmailTemplate
{
    private const string ColorBackground = "#F5F7FA";
    private const string ColorSurface = "#ffffff";
    private const string ColorText = "#1A1D21";
    private const string ColorMuted = "#6B7280";
    private const string ColorMutedLight = "#9CA3AF";
    private const string ColorGreen = "#059669";
    private const string ColorCardInner = "#F5F7FA";

    private static readonly TimeZoneInfo BrazilTimeZone = ResolveBrazilTimeZone();
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Build(Agendamento agendamento, string? platformLogoUrl = null)
    {
        var empresaNome = Encode(agendamento.Empresa?.Nome ?? "Clínica");
        var pacienteNome = Encode(agendamento.Paciente?.Nome ?? "Paciente");
        var unidadeNome = Encode(agendamento.Unidade?.Nome ?? "—");
        var unidadeEndereco = string.IsNullOrWhiteSpace(agendamento.Unidade?.Endereco)
            ? null
            : Encode(agendamento.Unidade!.Endereco!);
        var profissionalNome = Encode(agendamento.Funcionario?.Nome ?? "—");
        var procedimentoNomes = agendamento.ProcedimentosVinculados
            .OrderBy(item => item.CriadoEm)
            .Select(item => item.Procedimento?.Nome)
            .Where(nome => !string.IsNullOrWhiteSpace(nome))
            .Select(nome => Encode(nome!))
            .ToList();

        if (procedimentoNomes.Count == 0 && !string.IsNullOrWhiteSpace(agendamento.Procedimento?.Nome))
        {
            procedimentoNomes.Add(Encode(agendamento.Procedimento!.Nome));
        }

        var procedimento = procedimentoNomes.Count switch
        {
            0 => null,
            1 => procedimentoNomes[0],
            _ => string.Join(", ", procedimentoNomes)
        };

        var dataLabel = Encode(FormatDateLabel(agendamento.DataInicio));
        var horarioLabel = Encode(FormatTimeRange(agendamento.DataInicio, agendamento.DataFim));
        var logoBlock = BuildLogoBlock(agendamento.Empresa?.Logo, platformLogoUrl, empresaNome);
        var enderecoBlock = unidadeEndereco is null
            ? string.Empty
            : $"""<p style="margin:6px 0 0;font-size:14px;line-height:1.55;color:{ColorMuted};">{unidadeEndereco}</p>""";
        var procedimentoBlock = procedimento is null
            ? string.Empty
            : $"""<p style="margin:20px 0 0;font-size:16px;font-weight:600;line-height:1.4;color:{ColorText};">{procedimento}</p>""";
        var excecaoBlock = agendamento.ExcecaoHorario
            ? $"""<p style="margin:16px 0 0;font-size:13px;line-height:1.5;color:{ColorMuted};text-align:center;">Horário autorizado fora do expediente habitual da unidade.</p>"""
            : string.Empty;

        return $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Agendamento confirmado — BGD Clinical</title>
            </head>
            <body style="margin:0;padding:0;background-color:{ColorBackground};font-family:'Plus Jakarta Sans','Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:{ColorText};">
              <div style="display:none;max-height:0;overflow:hidden;">
                Olá, {pacienteNome}! Seu agendamento em {empresaNome} foi confirmado — {dataLabel}, {horarioLabel}.
              </div>
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:{ColorBackground};padding:40px 20px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:440px;background-color:{ColorSurface};border-radius:16px;">
                      <tr>
                        <td style="padding:40px 32px 48px;text-align:center;">
                          {logoBlock}
                          <p style="margin:36px 0 0;font-size:18px;font-weight:600;line-height:1.4;color:{ColorText};">
                            Olá, <strong>{pacienteNome}</strong>
                          </p>
                          <p style="margin:8px 0 0;font-size:15px;line-height:1.55;color:{ColorMuted};">
                            Seu agendamento foi confirmado.
                          </p>
                          <table role="presentation" cellspacing="0" cellpadding="0" border="0" align="center" style="margin:32px auto 28px;">
                            <tr>
                              <td align="center" style="width:72px;height:72px;background-color:{ColorBackground};border-radius:50%;font-size:32px;line-height:72px;color:{ColorGreen};">
                                ✓
                              </td>
                            </tr>
                          </table>
                          <p style="margin:0;font-size:20px;font-weight:600;line-height:1.35;color:{ColorText};letter-spacing:-0.02em;">
                            {dataLabel}
                          </p>
                          <p style="margin:10px 0 0;font-size:26px;font-weight:700;line-height:1.2;color:{ColorText};letter-spacing:-0.03em;">
                            {horarioLabel}
                          </p>
                          {excecaoBlock}
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:40px 0 0;background-color:{ColorCardInner};border-radius:16px;">
                            <tr>
                              <td style="padding:24px;text-align:left;">
                                <p style="margin:0;font-size:17px;font-weight:600;line-height:1.4;color:{ColorText};">{profissionalNome}</p>
                                <p style="margin:16px 0 0;font-size:15px;font-weight:500;line-height:1.45;color:{ColorText};">{unidadeNome}</p>
                                {enderecoBlock}
                                {procedimentoBlock}
                              </td>
                            </tr>
                          </table>
                          <p style="margin:36px 0 0;font-size:14px;line-height:1.65;color:{ColorMuted};">
                            Caso precise remarcar,<br />entre em contato com a clínica.
                          </p>
                          <p style="margin:48px 0 0;font-size:12px;font-weight:600;line-height:1.4;color:{ColorMutedLight};letter-spacing:0.02em;">
                            BGD Clinical
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string BuildLogoBlock(string? empresaLogoUrl, string? platformLogoUrl, string safeEmpresaNome)
    {
        var logoUrl = IsSafeAbsoluteUrl(empresaLogoUrl)
            ? empresaLogoUrl
            : IsSafeAbsoluteUrl(platformLogoUrl)
                ? platformLogoUrl
                : null;

        if (logoUrl is null)
        {
            return string.Empty;
        }

        return $"""
            <img src="{logoUrl}" alt="{safeEmpresaNome}" style="display:block;margin:0 auto;width:auto;height:88px;max-width:280px;max-height:96px;border:0;" />
            """;
    }

    private static bool IsSafeAbsoluteUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static string FormatDateLabel(DateTime value)
    {
        var local = ToBrazilLocal(value);
        var raw = local.ToString("dddd, d 'de' MMMM", PtBr);
        return PtBr.TextInfo.ToTitleCase(raw);
    }

    private static string FormatTimeRange(DateTime inicio, DateTime fim)
    {
        var localInicio = ToBrazilLocal(inicio);
        var localFim = ToBrazilLocal(fim);
        return $"{localInicio:HH:mm} — {localFim:HH:mm}";
    }

    private static DateTime ToBrazilLocal(DateTime value) =>
        TimeZoneInfo.ConvertTimeFromUtc(NormalizeToUtc(value), BrazilTimeZone);

    private static DateTime NormalizeToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static TimeZoneInfo ResolveBrazilTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
