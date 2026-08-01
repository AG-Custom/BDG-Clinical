namespace AG.CLINICAL.WebApi.Infrastructure.Auth;

public sealed class JwtSettings
{
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "AG.Clinical";

    public string Audience { get; set; } = "AG.Clinical";

    public int ExpirationMinutes { get; set; } = 480;
}
