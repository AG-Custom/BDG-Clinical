using AG.CLINICAL.Application.ClinicalRecords.Anamneses;
using AG.CLINICAL.Application.ClinicalRecords.BodyAssessments;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.ClinicalRecords.Encounters;
using AG.CLINICAL.Application.ClinicalRecords.Files;
using AG.CLINICAL.Application.ClinicalRecords.Notes;
using AG.CLINICAL.Application.ClinicalRecords.Nutrition;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.ClinicalEncounter;

[ApiController]
[Authorize]
[Route("api/clinical-encounters")]
public sealed class ClinicalEncounterController : ControllerBase
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    private readonly IGetClinicalEncountersService _get;
    private readonly IUpdateClinicalEncountersService _update;
    private readonly IFinalizeClinicalEncountersService _finalize;
    private readonly IListClinicalNotesService _listNotes;
    private readonly ICreateClinicalNotesService _createNotes;
    private readonly IUpdateClinicalNotesService _updateNotes;
    private readonly IListAnamneseRecordsService _listAnamneses;
    private readonly ICreateAnamneseRecordsService _createAnamneses;
    private readonly IUpdateAnamneseRecordsService _updateAnamneses;
    private readonly IListBodyAssessmentsService _listAssessments;
    private readonly ICreateBodyAssessmentsService _createAssessments;
    private readonly IGetBodyAssessmentsService _getAssessments;
    private readonly IUpdateBodyAssessmentsService _updateAssessments;
    private readonly IListClinicalAttachmentsService _listAttachments;
    private readonly IUploadClinicalAttachmentsService _uploadAttachments;
    private readonly IListComparativePhotosService _listPhotos;
    private readonly IUploadComparativePhotosService _uploadPhotos;
    private readonly ICompareComparativePhotosService _comparePhotos;
    private readonly IListEnergyCalculationsService _listVenta;
    private readonly ICreateEnergyCalculationsService _createVenta;
    private readonly IListPocketRulesService _listPocket;
    private readonly ICreatePocketRulesService _createPocket;

    public ClinicalEncounterController(
        IGetClinicalEncountersService get,
        IUpdateClinicalEncountersService update,
        IFinalizeClinicalEncountersService finalize,
        IListClinicalNotesService listNotes,
        ICreateClinicalNotesService createNotes,
        IUpdateClinicalNotesService updateNotes,
        IListAnamneseRecordsService listAnamneses,
        ICreateAnamneseRecordsService createAnamneses,
        IUpdateAnamneseRecordsService updateAnamneses,
        IListBodyAssessmentsService listAssessments,
        ICreateBodyAssessmentsService createAssessments,
        IGetBodyAssessmentsService getAssessments,
        IUpdateBodyAssessmentsService updateAssessments,
        IListClinicalAttachmentsService listAttachments,
        IUploadClinicalAttachmentsService uploadAttachments,
        IListComparativePhotosService listPhotos,
        IUploadComparativePhotosService uploadPhotos,
        ICompareComparativePhotosService comparePhotos,
        IListEnergyCalculationsService listVenta,
        ICreateEnergyCalculationsService createVenta,
        IListPocketRulesService listPocket,
        ICreatePocketRulesService createPocket)
    {
        _get = get;
        _update = update;
        _finalize = finalize;
        _listNotes = listNotes;
        _createNotes = createNotes;
        _updateNotes = updateNotes;
        _listAnamneses = listAnamneses;
        _createAnamneses = createAnamneses;
        _updateAnamneses = updateAnamneses;
        _listAssessments = listAssessments;
        _createAssessments = createAssessments;
        _getAssessments = getAssessments;
        _updateAssessments = updateAssessments;
        _listAttachments = listAttachments;
        _uploadAttachments = uploadAttachments;
        _listPhotos = listPhotos;
        _uploadPhotos = uploadPhotos;
        _comparePhotos = comparePhotos;
        _listVenta = listVenta;
        _createVenta = createVenta;
        _listPocket = listPocket;
        _createPocket = createPocket;
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _get.ExecuteAsync(id, cancellationToken);
        return result.IsFailure
            ? NotFound(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<ClinicalEncounterDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}")]
    [RequirePermission("prontuario.atendimento.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateClinicalEncounterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _update.ExecuteAsync(id, request, cancellationToken);
        return FailOrOk(result);
    }

    [HttpPost("{id:guid}/finalize")]
    [RequirePermission("prontuario.atendimento.editar")]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken cancellationToken)
    {
        var result = await _finalize.ExecuteAsync(id, cancellationToken);
        return FailOrOk(result);
    }

    [HttpGet("{id:guid}/notes")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListNotes(Guid id, CancellationToken cancellationToken)
    {
        var result = await _listNotes.ExecuteAsync(id, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<ClinicalNoteDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/notes")]
    [RequirePermission("prontuario.anotacao.criar")]
    public async Task<IActionResult> CreateNote(
        Guid id,
        [FromBody] CreateClinicalNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createNotes.ExecuteAsync(id, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<ClinicalNoteDto>(result.Value!, true));
    }

    [HttpPut("notes/{noteId:guid}")]
    [RequirePermission("prontuario.atendimento.editar")]
    public async Task<IActionResult> UpdateNote(
        Guid noteId,
        [FromBody] UpdateClinicalNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateNotes.ExecuteAsync(noteId, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<ClinicalNoteDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/anamneses")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListAnamneses(Guid id, CancellationToken cancellationToken)
    {
        var result = await _listAnamneses.ExecuteAsync(id, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<AnamneseRecordDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/anamneses")]
    [RequirePermission("prontuario.anamnese.editar")]
    public async Task<IActionResult> CreateAnamnese(
        Guid id,
        [FromBody] CreateAnamneseRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createAnamneses.ExecuteAsync(id, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<AnamneseRecordDto>(result.Value!, true));
    }

    [HttpPut("anamneses/{anamneseId:guid}")]
    [RequirePermission("prontuario.anamnese.editar")]
    public async Task<IActionResult> UpdateAnamnese(
        Guid anamneseId,
        [FromBody] UpdateAnamneseRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateAnamneses.ExecuteAsync(anamneseId, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<AnamneseRecordDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/body-assessments")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListAssessments(Guid id, CancellationToken cancellationToken)
    {
        var result = await _listAssessments.ExecuteAsync(Guid.Empty, id, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<BodyAssessmentDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/body-assessments")]
    [RequirePermission("prontuario.avaliacao.criar")]
    public async Task<IActionResult> CreateAssessment(
        Guid id,
        [FromBody] UpsertBodyAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createAssessments.ExecuteAsync(id, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<BodyAssessmentDto>(result.Value!, true));
    }

    [HttpGet("body-assessments/{assessmentId:guid}")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> GetAssessment(Guid assessmentId, CancellationToken cancellationToken)
    {
        var result = await _getAssessments.ExecuteAsync(assessmentId, cancellationToken);
        return result.IsFailure
            ? NotFound(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<BodyAssessmentDto>(result.Value!, true));
    }

    [HttpPut("body-assessments/{assessmentId:guid}")]
    [RequirePermission("prontuario.avaliacao.criar")]
    public async Task<IActionResult> UpdateAssessment(
        Guid assessmentId,
        [FromBody] UpsertBodyAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateAssessments.ExecuteAsync(assessmentId, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<BodyAssessmentDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/attachments")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListAttachments(Guid id, [FromQuery] string? tipo, CancellationToken cancellationToken)
    {
        var result = await _listAttachments.ExecuteAsync(id, tipo, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<ClinicalAttachmentDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/attachments")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.ClinicalAttachments)]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> UploadAttachment(
        Guid id,
        IFormFile file,
        [FromForm] string tipo,
        [FromForm] string nome,
        [FromForm] DateTime? dataDocumento,
        [FromForm] string? observacao,
        [FromForm] string? categoriaExame,
        [FromForm] string? categoriaDocumento,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, "Selecione um arquivo."));
        }

        await using var stream = file.OpenReadStream();
        var result = await _uploadAttachments.ExecuteAsync(
            id,
            new UploadClinicalAttachmentRequest(tipo, nome, dataDocumento, observacao, categoriaExame, categoriaDocumento),
            new ClinicalFileUpload(stream, file.ContentType, file.FileName, file.Length),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<ClinicalAttachmentDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/photos")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListPhotos(Guid id, CancellationToken cancellationToken)
    {
        var result = await _listPhotos.ExecuteAsync(Guid.Empty, id, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<ComparativePhotoDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/photos")]
    [RequirePermission("prontuario.foto.enviar")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> UploadPhoto(
        Guid id,
        IFormFile file,
        [FromForm] string categoria,
        [FromForm] DateTime? dataCaptura,
        [FromForm] string? observacao,
        [FromForm] Guid? avaliacaoCorporalId,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, "Selecione uma foto."));
        }

        await using var stream = file.OpenReadStream();
        var result = await _uploadPhotos.ExecuteAsync(
            id,
            categoria,
            dataCaptura,
            observacao,
            avaliacaoCorporalId,
            new ClinicalFileUpload(stream, file.ContentType, file.FileName, file.Length),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<ComparativePhotoDto>(result.Value!, true));
    }

    [HttpGet("photos/compare")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ComparePhotos(
        [FromQuery] Guid esquerdaId,
        [FromQuery] Guid direitaId,
        CancellationToken cancellationToken)
    {
        var result = await _comparePhotos.ExecuteAsync(esquerdaId, direitaId, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<PhotoComparisonDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/energy-calculations")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListVenta(Guid id, CancellationToken cancellationToken)
    {
        var result = await _listVenta.ExecuteAsync(id, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<EnergyCalculationDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/energy-calculations")]
    [RequirePermission("prontuario.atendimento.editar")]
    public async Task<IActionResult> CreateVenta(
        Guid id,
        [FromBody] CreateEnergyCalculationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createVenta.ExecuteAsync(id, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<EnergyCalculationDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/pocket-rules")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListPocket(Guid id, CancellationToken cancellationToken)
    {
        var result = await _listPocket.ExecuteAsync(id, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<PocketRuleDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/pocket-rules")]
    [RequirePermission("prontuario.atendimento.editar")]
    public async Task<IActionResult> CreatePocket(
        Guid id,
        [FromBody] CreatePocketRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPocket.ExecuteAsync(id, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<PocketRuleDto>(result.Value!, true));
    }

    private IActionResult FailOrOk(Application.Common.Result<ClinicalEncounterDto> result)
    {
        if (result.IsFailure)
        {
            var status = result.Error == "Atendimento não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            return StatusCode(status, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ClinicalEncounterDto>(result.Value!, true));
    }
}
