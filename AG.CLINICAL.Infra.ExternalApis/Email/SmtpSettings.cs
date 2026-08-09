namespace AG.CLINICAL.Infra.ExternalApis.Email;

public sealed class SmtpSettings
{
    public bool Enabled { get; set; } = true;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "AG Clinical";

    public bool UseSsl { get; set; } = true;
}
