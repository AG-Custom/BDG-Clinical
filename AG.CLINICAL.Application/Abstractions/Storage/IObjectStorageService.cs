namespace AG.CLINICAL.Application.Abstractions.Storage;

public sealed record ObjectStorageUploadRequest(
    string ObjectKey,
    Stream Content,
    string ContentType,
    long ContentLength);

public interface IObjectStorageService
{
    Task<string> UploadAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteByObjectKeyAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    string BuildPublicUrl(string objectKey);

    bool TryExtractObjectKeyFromPublicUrl(string publicUrl, out string objectKey);
}
