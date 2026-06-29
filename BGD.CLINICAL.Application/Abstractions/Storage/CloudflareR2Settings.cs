namespace BGD.CLINICAL.Application.Abstractions.Storage;

public sealed class CloudflareR2Settings
{
    public string AccountId { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public string ServiceUrl { get; set; } = string.Empty;

    public string PublicBaseUrl { get; set; } = string.Empty;

    public long MaxLogoSizeBytes { get; set; } = 2 * 1024 * 1024;

    public long MaxAttachmentSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } =
    [
        "image/png",
        "image/jpeg",
        "image/webp",
    ];

    public string[] AllowedAttachmentContentTypes { get; set; } =
    [
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/webp",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    ];

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BucketName)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey)
        && !string.IsNullOrWhiteSpace(ServiceUrl)
        && !string.IsNullOrWhiteSpace(PublicBaseUrl);
}
