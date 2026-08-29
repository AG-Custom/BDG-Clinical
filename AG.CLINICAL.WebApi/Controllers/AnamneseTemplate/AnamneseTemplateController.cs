using AG.CLINICAL.Application.ClinicalRecords.Anamneses;
using AG.CLINICAL.Application.ClinicalRecords.Dtos;
using AG.CLINICAL.WebApi.Authorization;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.AnamneseTemplate;

[ApiController]
[Authorize]
[Route("api/anamnese-templates")]
public sealed class AnamneseTemplateController : ControllerBase
{
    private readonly IListAnamneseTemplatesService _list;
    private readonly IGetAnamneseTemplatesService _get;
    private readonly ICreateAnamneseTemplatesService _create;
    private readonly IUpdateAnamneseTemplatesService _update;
    private readonly IDeactivateAnamneseTemplatesService _deactivate;

    public AnamneseTemplateController(
        IListAnamneseTemplatesService list,
        IGetAnamneseTemplatesService get,
        ICreateAnamneseTemplatesService create,
        IUpdateAnamneseTemplatesService update,
        IDeactivateAnamneseTemplatesService deactivate)
    {
        _list = list;
        _get = get;
        _create = create;
        _update = update;
        _deactivate = deactivate;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.AnamneseTemplates)]
    public async Task<IActionResult> List([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _list.ExecuteAsync(includeInactive, cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<AnamneseTemplateDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.AnamneseTemplates)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _get.ExecuteAsync(id, cancellationToken);
        return result.IsFailure
            ? NotFound(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<AnamneseTemplateDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("prontuario.modelo_anamnese.gerenciar")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAnamneseTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _create.ExecuteAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, new ApiResponse<AnamneseTemplateDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("prontuario.modelo_anamnese.gerenciar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAnamneseTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _update.ExecuteAsync(id, request, cancellationToken);
        return result.IsFailure
            ? BadRequest(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<AnamneseTemplateDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("prontuario.modelo_anamnese.gerenciar")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivate.ExecuteAsync(id, cancellationToken);
        return result.IsFailure
            ? NotFound(new ApiResponse<object?>(null!, false, result.Error))
            : Ok(new ApiResponse<AnamneseTemplateDto>(result.Value!, true));
    }
}
