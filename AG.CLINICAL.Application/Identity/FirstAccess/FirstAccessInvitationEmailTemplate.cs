using System.Net;

namespace AG.CLINICAL.Application.Identity.FirstAccess;

internal static class FirstAccessInvitationEmailTemplate
{
    private const string ColorBackground = "#F5F7FA";
    private const string ColorSurface = "#ffffff";
    private const string ColorText = "#1A1D21";
    private const string ColorMuted = "#6B7280";
    private const string ColorMutedLight = "#9CA3AF";
    private const string ColorGreen = "#059669";
    private const string ColorCardInner = "#F5F7FA";

    public static string Build(
        string nome,
        string empresaNome,
        string firstAccessUrl,
        int expirationHours,
        string? empresaLogoUrl = null,
        string? platformLogoUrl = null)
    {
        var safeNome = Encode(nome);
        var safeEmpresa = Encode(empresaNome);
        var safeUrl = Encode(firstAccessUrl);
        var validityLabel = FormatValidity(expirationHours);
        var logoBlock = BuildLogoBlock(empresaLogoUrl, platformLogoUrl, safeEmpresa);

        return $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Primeiro acesso — AG Clinical</title>
            </head>
            <body style="margin:0;padding:0;background-color:{ColorBackground};font-family:'Plus Jakarta Sans','Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:{ColorText};">
              <div style="display:none;max-height:0;overflow:hidden;">
                Olá, {safeNome}! Você foi convidado(a) para acessar {safeEmpresa}. Conclua seu primeiro acesso em {validityLabel}.
              </div>
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:{ColorBackground};padding:40px 20px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:440px;background-color:{ColorSurface};border-radius:16px;">
                      <tr>
                        <td style="padding:40px 32px 48px;text-align:center;">
                          {logoBlock}
                          <p style="margin:36px 0 0;font-size:18px;font-weight:600;line-height:1.4;color:{ColorText};">
                            Olá, <strong>{safeNome}</strong>
                          </p>
                          <p style="margin:8px 0 0;font-size:15px;line-height:1.55;color:{ColorMuted};">
                            Você foi convidado(a) a acessar a plataforma.
                          </p>
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:32px 0 0;background-color:{ColorCardInner};border-radius:16px;">
                            <tr>
                              <td style="padding:24px;text-align:left;">
                                <p style="margin:0;font-size:17px;font-weight:600;line-height:1.4;color:{ColorText};">{safeEmpresa}</p>
                                <p style="margin:16px 0 0;font-size:14px;line-height:1.7;color:{ColorMuted};">
                                  1. Clique no botão abaixo<br />
                                  2. Informe o e-mail cadastrado pela clínica<br />
                                  3. Crie sua senha e entre no sistema
                                </p>
                              </td>
                            </tr>
                          </table>
                          <table role="presentation" cellspacing="0" cellpadding="0" border="0" align="center" style="margin:32px auto 0;">
                            <tr>
                              <td align="center" bgcolor="{ColorGreen}" style="border-radius:12px;background-color:{ColorGreen};">
                                <a href="{safeUrl}" target="_blank" style="display:inline-block;padding:14px 28px;font-size:15px;font-weight:600;line-height:1.2;color:#ffffff;text-decoration:none;border-radius:12px;">
                                  Concluir primeiro acesso
                                </a>
                              </td>
                            </tr>
                          </table>
                          <p style="margin:28px 0 0;font-size:13px;line-height:1.55;color:{ColorMuted};">
                            Se o botão não funcionar, copie e cole este link no navegador:
                          </p>
                          <p style="margin:8px 0 0;font-size:12px;line-height:1.55;word-break:break-all;">
                            <a href="{safeUrl}" style="color:{ColorGreen};text-decoration:underline;">{safeUrl}</a>
                          </p>
                          <p style="margin:32px 0 0;font-size:13px;line-height:1.6;color:{ColorMuted};">
                            Este convite expira em <strong style="color:{ColorText};">{validityLabel}</strong>.<br />
                            O link é pessoal — se não esperava este e-mail, ignore-o.
                          </p>
                          <p style="margin:48px 0 0;font-size:12px;font-weight:600;line-height:1.4;color:{ColorMutedLight};letter-spacing:0.02em;">
                            AG Clinical
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

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string FormatValidity(int expirationHours)
    {
        if (expirationHours <= 0)
        {
            return "pouco tempo";
        }

        if (expirationHours % 24 == 0)
        {
            var days = expirationHours / 24;
            return days == 1 ? "1 dia" : $"{days} dias";
        }

        return expirationHours == 1 ? "1 hora" : $"{expirationHours} horas";
    }
}
