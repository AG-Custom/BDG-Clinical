using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Abstractions.Storage;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace AG.CLINICAL.Application.Core.Companies;

public sealed record CompanyLogoUpload(
    Stream Content,
    string ContentType,
    string FileName,
    long Length);

public interface IUploadCompanyLogoService
{
    Task<Result<CompanyDto>> ExecuteAsync(
        CompanyLogoUpload upload,
        CancellationToken cancellationToken = default);
}

public sealed class UploadCompanyLogoService : IUploadCompanyLogoService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICompaniesRepository _companiesRepository;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudflareR2Settings _r2Settings;

    public UploadCompanyLogoService(
        ICurrentTenantContext tenantContext,
        ICompaniesRepository companiesRepository,
        IObjectStorageService objectStorageService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IOptions<CloudflareR2Settings> r2Settings)
    {
        _tenantContext = tenantContext;
        _companiesRepository = companiesRepository;
        _objectStorageService = objectStorageService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _r2Settings = r2Settings.Value;
    }

    public async Task<Result<CompanyDto>> ExecuteAsync(
        CompanyLogoUpload upload,
        CancellationToken cancellationToken = default)
    {
        if (!_r2Settings.IsConfigured)
        {
            return Result<CompanyDto>.Failure("Armazenamento de arquivos não configurado.");
        }

        var validationError = ValidateUpload(upload);
        if (validationError is not null)
        {
            return Result<CompanyDto>.Failure(validationError);
        }

        var empresaId = _tenantContext.EmpresaId;
        var empresa = await _companiesRepository.GetByIdAsync(empresaId, cancellationToken);

        if (empresa is null)
        {
            return Result<CompanyDto>.Failure("Empresa não encontrada.");
        }

        if (!empresa.Ativo)
        {
            return Result<CompanyDto>.Failure("Não é possível editar uma empresa inativa.");
        }

        var extension = ResolveExtension(upload.ContentType, upload.FileName);
        if (extension is null)
        {
            return Result<CompanyDto>.Failure("Formato não suportado. Envie PNG, JPEG ou WebP.");
        }

        var objectKey = $"companies/{empresaId}/logo{extension}";
        var previousLogo = empresa.Logo;

        string logoUrl;
        try
        {
            logoUrl = await _objectStorageService.UploadAsync(
                new ObjectStorageUploadRequest(
                    objectKey,
                    upload.Content,
                    upload.ContentType,
                    upload.Length),
                cancellationToken);
        }
        catch (Exception)
        {
            return Result<CompanyDto>.Failure("Não foi possível enviar a logo. Tente novamente.");
        }

        var dadosAnteriores = CompaniesAuditSerializer.Serialize(empresa);

        try
        {
            empresa.UpdateDetails(
                empresa.Nome,
                empresa.Cnpj,
                empresa.Telefone,
                empresa.Email,
                empresa.CorPrincipal,
                logoUrl);
        }
        catch (DomainException ex)
        {
            return Result<CompanyDto>.Failure(ex.Message);
        }

        _companiesRepository.Update(empresa);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousLogo)
            && !string.Equals(previousLogo, logoUrl, StringComparison.OrdinalIgnoreCase)
            && _objectStorageService.TryExtractObjectKeyFromPublicUrl(previousLogo, out var previousKey)
            && !string.Equals(previousKey, objectKey, StringComparison.OrdinalIgnoreCase))
        {
            await _objectStorageService.DeleteByObjectKeyAsync(previousKey, cancellationToken);
        }

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Domain.Entities.Empresa),
            empresa.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: CompaniesAuditSerializer.Serialize(empresa),
            cancellationToken: cancellationToken);

        return Result<CompanyDto>.Success(CompaniesMapper.Map(empresa));
    }

    private string? ValidateUpload(CompanyLogoUpload upload)
    {
        if (upload.Length <= 0)
        {
            return "Selecione um arquivo de imagem.";
        }

        if (upload.Length > _r2Settings.MaxLogoSizeBytes)
        {
            var maxMb = _r2Settings.MaxLogoSizeBytes / (1024 * 1024);
            return $"A logo deve ter no máximo {maxMb} MB.";
        }

        var contentType = upload.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!_r2Settings.AllowedContentTypes.Contains(contentType))
        {
            return "Formato não suportado. Envie PNG, JPEG ou WebP.";
        }

        if (ResolveExtension(contentType, upload.FileName) is null)
        {
            return "Formato não suportado. Envie PNG, JPEG ou WebP.";
        }

        return null;
    }

    private static string? ResolveExtension(string contentType, string fileName)
    {
        return contentType.Trim().ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            _ => ResolveExtensionFromFileName(fileName),
        };
    }

    private static string? ResolveExtensionFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).Trim().ToLowerInvariant();

        return extension switch
        {
            ".png" => ".png",
            ".jpg" or ".jpeg" => ".jpg",
            ".webp" => ".webp",
            _ => null,
        };
    }
}
