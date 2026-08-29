using AG.CLINICAL.Application.ClinicalRecords.BodyAssessments;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.Application.ClinicalRecords.Encounters;
using AG.CLINICAL.Application.ClinicalRecords.Files;
using AG.CLINICAL.Application.ClinicalRecords.MedicalRecords;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.MedicalRecord;

[ApiController]
[Authorize]
public sealed class MedicalRecordController : ControllerBase
{
    private readonly IGetOrCreateMedicalRecordsService _getOrCreate;
    private readonly IUpdateMedicalRecordsService _update;
    private readonly IGetMedicalRecordSummariesService _summaries;
    private readonly IListClinicalEncountersService _listEncounters;
    private readonly ICreateClinicalEncountersService _createEncounters;
    private readonly IListBodyAssessmentsService _listAssessments;
    private readonly IGetBodyEvolutionService _evolution;
    private readonly IListComparativePhotosService _listPhotos;

    public MedicalRecordController(
        IGetOrCreateMedicalRecordsService getOrCreate,
        IUpdateMedicalRecordsService update,
        IGetMedicalRecordSummariesService summaries,
        IListClinicalEncountersService listEncounters,
        ICreateClinicalEncountersService createEncounters,
        IListBodyAssessmentsService listAssessments,
        IGetBodyEvolutionService evolution,
        IListComparativePhotosService listPhotos)
    {
        _getOrCreate = getOrCreate;
        _update = update;
        _summaries = summaries;
        _listEncounters = listEncounters;
        _createEncounters = createEncounters;
        _listAssessments = listAssessments;
        _evolution = evolution;
        _listPhotos = listPhotos;
    }

    [HttpGet("api/patients/{patientId:guid}/medical-record")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> Get(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _getOrCreate.ExecuteAsync(patientId, cancellationToken);
        return result.IsFailure
            ? NotFound(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<MedicalRecordDto>(result.Value!, true));
    }

    [HttpPatch("api/patients/{patientId:guid}/medical-record")]
    [RequirePermission("prontuario.atendimento.editar")]
    public async Task<IActionResult> Update(
        Guid patientId,
        [FromBody] UpdateMedicalRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _update.ExecuteAsync(patientId, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<MedicalRecordDto>(result.Value!, true));
    }

    [HttpGet("api/patients/{patientId:guid}/medical-record/summary")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> Summary(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _summaries.ExecuteAsync(patientId, cancellationToken);
        return result.IsFailure
            ? NotFound(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<MedicalRecordSummaryDto>(result.Value!, true));
    }

    [HttpGet("api/patients/{patientId:guid}/clinical-encounters")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListEncounters(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _listEncounters.ExecuteAsync(patientId, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<ClinicalEncounterDto>>(result.Value!, true));
    }

    [HttpPost("api/patients/{patientId:guid}/clinical-encounters")]
    [RequirePermission("prontuario.atendimento.criar")]
    public async Task<IActionResult> CreateEncounter(
        Guid patientId,
        [FromBody] CreateClinicalEncounterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createEncounters.ExecuteAsync(patientId, request, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Created(
            $"/api/clinical-encounters/{result.Value!.Id}",
            new ApiResponse<ClinicalEncounterDto>(result.Value, true));
    }

    [HttpGet("api/patients/{patientId:guid}/body-assessments")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> ListAssessments(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _listAssessments.ExecuteAsync(patientId, null, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<BodyAssessmentDto>>(result.Value!, true));
    }

    [HttpGet("api/patients/{patientId:guid}/body-evolution")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> Evolution(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _evolution.ExecuteAsync(patientId, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<BodyEvolutionPointDto>>(result.Value!, true));
    }

    [HttpGet("api/patients/{patientId:guid}/photos")]
    [RequirePermission("prontuario.visualizar")]
    public async Task<IActionResult> Photos(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _listPhotos.ExecuteAsync(patientId, null, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<ComparativePhotoDto>>(result.Value!, true));
    }
}
