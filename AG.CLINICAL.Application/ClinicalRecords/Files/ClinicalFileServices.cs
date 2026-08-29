using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Abstractions.Storage;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace AG.CLINICAL.Application.ClinicalRecords.Files;

public sealed record ClinicalFileUpload(Stream Content, string ContentType, string FileName, long Length);

public interface IListClinicalAttachmentsService
{
    Task<Result<IReadOnlyList<ClinicalAttachmentDto>>> ExecuteAsync(
        Guid atendimentoId,
        string? tipo,
        CancellationToken cancellationToken = default);
}

public sealed class ListClinicalAttachmentsService : IListClinicalAttachmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalFilesRepository _filesRepository;
    private readonly IObjectStorageService _objectStorageService;

    public ListClinicalAttachmentsService(
        ICurrentTenantContext tenantContext,
        IClinicalFilesRepository filesRepository,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _filesRepository = filesRepository;
        _objectStorageService = objectStorageService;
    }

    public async Task<Result<IReadOnlyList<ClinicalAttachmentDto>>> ExecuteAsync(
        Guid atendimentoId,
        string? tipo,
        CancellationToken cancellationToken = default)
    {
        var tipoEnum = ClinicalRecordEnums.ParseOptional<TipoAnexoClinico>(tipo);
        var itens = await _filesRepository.ListAnexosByAtendimentoAsync(
            _tenantContext.EmpresaId,
            atendimentoId,
            tipoEnum,
            cancellationToken);

        return Result<IReadOnlyList<ClinicalAttachmentDto>>.Success(
            itens.Select(item => ClinicalRecordMapper.Map(item, _objectStorageService.BuildPublicUrl(item.ObjectKey))).ToList());
    }
}

public interface IUploadClinicalAttachmentsService
{
    Task<Result<ClinicalAttachmentDto>> ExecuteAsync(
        Guid atendimentoId,
        UploadClinicalAttachmentRequest meta,
        ClinicalFileUpload upload,
        CancellationToken cancellationToken = default);
}

public sealed class UploadClinicalAttachmentsService : IUploadClinicalAttachmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IClinicalFilesRepository _filesRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudflareR2Settings _r2Settings;

    public UploadClinicalAttachmentsService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IClinicalFilesRepository filesRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IObjectStorageService objectStorageService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IOptions<CloudflareR2Settings> r2Settings)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _filesRepository = filesRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _objectStorageService = objectStorageService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _r2Settings = r2Settings.Value;
    }

    public async Task<Result<ClinicalAttachmentDto>> ExecuteAsync(
        Guid atendimentoId,
        UploadClinicalAttachmentRequest meta,
        ClinicalFileUpload upload,
        CancellationToken cancellationToken = default)
    {
        if (!_r2Settings.IsConfigured)
        {
            return Result<ClinicalAttachmentDto>.Failure("Armazenamento de arquivos não configurado.");
        }

        if (upload.Length <= 0 || upload.Length > _r2Settings.MaxAttachmentSizeBytes)
        {
            return Result<ClinicalAttachmentDto>.Failure("Arquivo inválido ou maior que o limite permitido.");
        }

        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimentoId, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<ClinicalAttachmentDto>.Failure("Atendimento não encontrado.");
        }

        try
        {
            var tipo = ClinicalRecordEnums.Parse<TipoAnexoClinico>(meta.Tipo, "Tipo de anexo inválido.");
            var contentType = upload.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
            var extension = ResolveExtension(contentType, upload.FileName);
            if (extension is null || !IsAllowed(tipo, contentType))
            {
                return Result<ClinicalAttachmentDto>.Failure("Formato não suportado. Envie PDF, JPG ou PNG.");
            }

            var objectKey = $"companies/{empresaId}/patients/{atendimento.PacienteId}/encounters/{atendimento.Id}/{Guid.NewGuid():N}{extension}";
            await _objectStorageService.UploadAsync(
                new ObjectStorageUploadRequest(objectKey, upload.Content, contentType, upload.Length),
                cancellationToken);

            var anexo = AnexoClinico.Create(
                empresaId,
                atendimento.Id,
                atendimento.PacienteId,
                atendimento.UnidadeId,
                atendimento.FuncionarioId,
                tipo,
                meta.Nome,
                meta.DataDocumento ?? DateTime.UtcNow,
                meta.Observacao,
                meta.CategoriaExame,
                meta.CategoriaDocumento,
                upload.FileName,
                contentType,
                objectKey,
                upload.Length);

            await _filesRepository.AddAnexoAsync(anexo, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    tipo == TipoAnexoClinico.Exame ? TipoEventoClinico.ExameEnviado : TipoEventoClinico.DocumentoAnexado,
                    tipo == TipoAnexoClinico.Exame ? "Exame enviado" : "Documento anexado",
                    anexo.Nome,
                    nameof(AnexoClinico),
                    anexo.Id,
                    atendimento.FuncionarioId,
                    atendimento.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AnexoClinico),
                anexo.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { anexo.Nome, anexo.Tipo }),
                cancellationToken: cancellationToken);

            return Result<ClinicalAttachmentDto>.Success(
                ClinicalRecordMapper.Map(anexo, _objectStorageService.BuildPublicUrl(objectKey)));
        }
        catch (DomainException exception)
        {
            return Result<ClinicalAttachmentDto>.Failure(exception.Message);
        }
        catch (Exception)
        {
            return Result<ClinicalAttachmentDto>.Failure("Não foi possível enviar o arquivo. Tente novamente.");
        }
    }

    private static bool IsAllowed(TipoAnexoClinico tipo, string contentType)
    {
        return contentType is "application/pdf" or "image/jpeg" or "image/png" or "image/webp";
    }

    private static string? ResolveExtension(string contentType, string fileName)
    {
        return contentType switch
        {
            "application/pdf" => ".pdf",
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            _ => Path.GetExtension(fileName).ToLowerInvariant() is ".pdf" or ".png" or ".jpg" or ".jpeg" or ".webp"
                ? Path.GetExtension(fileName).ToLowerInvariant()
                : null
        };
    }
}

public interface IListComparativePhotosService
{
    Task<Result<IReadOnlyList<ComparativePhotoDto>>> ExecuteAsync(
        Guid pacienteId,
        Guid? atendimentoId,
        CancellationToken cancellationToken = default);
}

public sealed class ListComparativePhotosService : IListComparativePhotosService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalFilesRepository _filesRepository;
    private readonly IObjectStorageService _objectStorageService;

    public ListComparativePhotosService(
        ICurrentTenantContext tenantContext,
        IClinicalFilesRepository filesRepository,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _filesRepository = filesRepository;
        _objectStorageService = objectStorageService;
    }

    public async Task<Result<IReadOnlyList<ComparativePhotoDto>>> ExecuteAsync(
        Guid pacienteId,
        Guid? atendimentoId,
        CancellationToken cancellationToken = default)
    {
        var itens = atendimentoId.HasValue
            ? await _filesRepository.ListFotosByAtendimentoAsync(_tenantContext.EmpresaId, atendimentoId.Value, cancellationToken)
            : await _filesRepository.ListFotosByPacienteAsync(_tenantContext.EmpresaId, pacienteId, cancellationToken);

        return Result<IReadOnlyList<ComparativePhotoDto>>.Success(
            itens.Select(item => ClinicalRecordMapper.Map(item, _objectStorageService.BuildPublicUrl(item.ObjectKey))).ToList());
    }
}

public interface IUploadComparativePhotosService
{
    Task<Result<ComparativePhotoDto>> ExecuteAsync(
        Guid atendimentoId,
        string categoria,
        DateTime? dataCaptura,
        string? observacao,
        Guid? avaliacaoCorporalId,
        ClinicalFileUpload upload,
        CancellationToken cancellationToken = default);
}

public sealed class UploadComparativePhotosService : IUploadComparativePhotosService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IClinicalFilesRepository _filesRepository;
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudflareR2Settings _r2Settings;

    public UploadComparativePhotosService(
        ICurrentTenantContext tenantContext,
        IClinicalEncountersRepository encountersRepository,
        IClinicalFilesRepository filesRepository,
        IMedicalRecordsRepository medicalRecordsRepository,
        IObjectStorageService objectStorageService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork,
        IOptions<CloudflareR2Settings> r2Settings)
    {
        _tenantContext = tenantContext;
        _encountersRepository = encountersRepository;
        _filesRepository = filesRepository;
        _medicalRecordsRepository = medicalRecordsRepository;
        _objectStorageService = objectStorageService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
        _r2Settings = r2Settings.Value;
    }

    public async Task<Result<ComparativePhotoDto>> ExecuteAsync(
        Guid atendimentoId,
        string categoria,
        DateTime? dataCaptura,
        string? observacao,
        Guid? avaliacaoCorporalId,
        ClinicalFileUpload upload,
        CancellationToken cancellationToken = default)
    {
        if (!_r2Settings.IsConfigured)
        {
            return Result<ComparativePhotoDto>.Failure("Armazenamento de arquivos não configurado.");
        }

        var contentType = upload.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (contentType is not ("image/jpeg" or "image/png" or "image/webp"))
        {
            return Result<ComparativePhotoDto>.Failure("Envie uma imagem JPG, PNG ou WebP.");
        }

        var empresaId = _tenantContext.EmpresaId;
        var atendimento = await _encountersRepository.GetByIdAndEmpresaIdAsync(atendimentoId, empresaId, cancellationToken);
        if (atendimento is null)
        {
            return Result<ComparativePhotoDto>.Failure("Atendimento não encontrado.");
        }

        try
        {
            var categoriaEnum = ClinicalRecordEnums.Parse<CategoriaFotoComparativa>(categoria, "Categoria de foto inválida.");
            var extension = contentType == "image/png" ? ".png" : contentType == "image/webp" ? ".webp" : ".jpg";
            var objectKey = $"companies/{empresaId}/patients/{atendimento.PacienteId}/photos/{Guid.NewGuid():N}{extension}";

            await _objectStorageService.UploadAsync(
                new ObjectStorageUploadRequest(objectKey, upload.Content, contentType, upload.Length),
                cancellationToken);

            var foto = FotoComparativa.Create(
                empresaId,
                atendimento.Id,
                atendimento.PacienteId,
                atendimento.UnidadeId,
                atendimento.FuncionarioId,
                categoriaEnum,
                dataCaptura ?? DateTime.UtcNow,
                observacao,
                avaliacaoCorporalId,
                upload.FileName,
                contentType,
                objectKey,
                upload.Length);

            await _filesRepository.AddFotoAsync(foto, cancellationToken);
            await _medicalRecordsRepository.AddEventoAsync(
                EventoClinico.Create(
                    empresaId,
                    atendimento.PacienteId,
                    atendimento.Id,
                    TipoEventoClinico.FotoAdicionada,
                    "Foto adicionada",
                    categoriaEnum.ToString(),
                    nameof(FotoComparativa),
                    foto.Id,
                    atendimento.FuncionarioId,
                    atendimento.UnidadeId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(FotoComparativa),
                foto.Id,
                AcaoAuditoria.Criar,
                dadosNovos: ClinicalRecordAudit.Summary(new { foto.Categoria }),
                cancellationToken: cancellationToken);

            return Result<ComparativePhotoDto>.Success(
                ClinicalRecordMapper.Map(foto, _objectStorageService.BuildPublicUrl(objectKey)));
        }
        catch (DomainException exception)
        {
            return Result<ComparativePhotoDto>.Failure(exception.Message);
        }
    }
}

public interface ICompareComparativePhotosService
{
    Task<Result<PhotoComparisonDto>> ExecuteAsync(
        Guid esquerdaId,
        Guid direitaId,
        CancellationToken cancellationToken = default);
}

public sealed class CompareComparativePhotosService : ICompareComparativePhotosService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IClinicalFilesRepository _filesRepository;
    private readonly IObjectStorageService _objectStorageService;

    public CompareComparativePhotosService(
        ICurrentTenantContext tenantContext,
        IClinicalFilesRepository filesRepository,
        IObjectStorageService objectStorageService)
    {
        _tenantContext = tenantContext;
        _filesRepository = filesRepository;
        _objectStorageService = objectStorageService;
    }

    public async Task<Result<PhotoComparisonDto>> ExecuteAsync(
        Guid esquerdaId,
        Guid direitaId,
        CancellationToken cancellationToken = default)
    {
        var esquerda = await _filesRepository.GetFotoByIdAsync(esquerdaId, _tenantContext.EmpresaId, cancellationToken);
        var direita = await _filesRepository.GetFotoByIdAsync(direitaId, _tenantContext.EmpresaId, cancellationToken);
        if (esquerda is null || direita is null)
        {
            return Result<PhotoComparisonDto>.Failure("Selecione duas fotos válidas para comparar.");
        }

        return Result<PhotoComparisonDto>.Success(new PhotoComparisonDto(
            ClinicalRecordMapper.Map(esquerda, _objectStorageService.BuildPublicUrl(esquerda.ObjectKey)),
            ClinicalRecordMapper.Map(direita, _objectStorageService.BuildPublicUrl(direita.ObjectKey))));
    }
}
