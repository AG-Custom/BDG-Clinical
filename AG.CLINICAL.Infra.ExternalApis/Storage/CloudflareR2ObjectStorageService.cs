using Amazon.S3;
using Amazon.S3.Model;
using AG.CLINICAL.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AG.CLINICAL.Infra.ExternalApis.Storage;

public sealed class CloudflareR2ObjectStorageService : IObjectStorageService
{
    private readonly CloudflareR2Settings _settings;
    private readonly ILogger<CloudflareR2ObjectStorageService> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly string _publicBaseUrl;

    public CloudflareR2ObjectStorageService(
        IOptions<CloudflareR2Settings> settings,
        ILogger<CloudflareR2ObjectStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _publicBaseUrl = _settings.PublicBaseUrl.TrimEnd('/');

        var config = new AmazonS3Config
        {
            ServiceURL = _settings.ServiceUrl.TrimEnd('/'),
            ForcePathStyle = true,
            AuthenticationRegion = "auto",
        };

        _s3Client = new AmazonS3Client(
            _settings.AccessKeyId,
            _settings.SecretAccessKey,
            config);
    }

    public async Task<string> UploadAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = request.ObjectKey,
            InputStream = request.Content,
            ContentType = request.ContentType,
            AutoCloseStream = false,
            DisablePayloadSigning = true,
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        return BuildPublicUrl(request.ObjectKey);
    }

    public async Task DeleteByObjectKeyAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(
                _settings.BucketName,
                objectKey,
                cancellationToken);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao remover objeto {ObjectKey} do R2.", objectKey);
        }
    }

    public string BuildPublicUrl(string objectKey)
    {
        return $"{_publicBaseUrl}/{objectKey.TrimStart('/')}";
    }

    public bool TryExtractObjectKeyFromPublicUrl(string publicUrl, out string objectKey)
    {
        objectKey = string.Empty;

        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return false;
        }

        var normalizedUrl = publicUrl.Trim();
        var prefix = _publicBaseUrl + "/";

        if (!normalizedUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        objectKey = normalizedUrl[prefix.Length..];
        return !string.IsNullOrWhiteSpace(objectKey);
    }
}
