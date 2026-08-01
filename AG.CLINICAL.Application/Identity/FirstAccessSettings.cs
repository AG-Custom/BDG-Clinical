namespace AG.CLINICAL.Application.Identity;

public sealed class FirstAccessSettings
{
    public string FrontendBaseUrl { get; set; } = string.Empty;

    public string Path { get; set; } = "/primeiro-acesso";

    public int TokenExpirationHours { get; set; } = 168;

    public string? PlatformLogoUrl { get; set; }
}
