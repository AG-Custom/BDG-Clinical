using BGD.CLINICAL.Application.Core.Companies;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity.Dtos;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Company;

[ApiController]
[Authorize]
[Route("api/companies")]
public sealed class CompanyController : ControllerBase
{
    private const long MaxLogoUploadBytes = 2 * 1024 * 1024;

    private readonly IGetCurrentCompanyService _getCurrentCompanyService;
    private readonly IListUserCompaniesService _listUserCompaniesService;
    private readonly ICreateCompanyService _createCompanyService;
    private readonly IUpdateCurrentCompanyService _updateCurrentCompanyService;
    private readonly IReactivateCompanyService _reactivateCompanyService;
    private readonly IUploadCompanyLogoService _uploadCompanyLogoService;

    public CompanyController(
        IGetCurrentCompanyService getCurrentCompanyService,
        IListUserCompaniesService listUserCompaniesService,
        ICreateCompanyService createCompanyService,
        IUpdateCurrentCompanyService updateCurrentCompanyService,
        IReactivateCompanyService reactivateCompanyService,
        IUploadCompanyLogoService uploadCompanyLogoService)
    {
        _getCurrentCompanyService = getCurrentCompanyService;
        _listUserCompaniesService = listUserCompaniesService;
        _createCompanyService = createCompanyService;
        _updateCurrentCompanyService = updateCurrentCompanyService;
        _reactivateCompanyService = reactivateCompanyService;
        _uploadCompanyLogoService = uploadCompanyLogoService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _listUserCompaniesService.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<UserCompanyDto>>(result.Value!, true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createCompanyService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AuthResponse>(result.Value!, true));
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _getCurrentCompanyService.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<CompanyDto>(result.Value!, true));
    }

    [HttpPut("current")]
    [RequirePermission("empresa.editar")]
    public async Task<IActionResult> UpdateCurrent(
        [FromBody] UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateCurrentCompanyService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Empresa não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<CompanyDto>(result.Value!, true));
    }

    [HttpPatch("{empresaId:guid}/reactivate")]
    [RequirePermission("empresa.editar")]
    public async Task<IActionResult> Reactivate(Guid empresaId, CancellationToken cancellationToken)
    {
        var result = await _reactivateCompanyService.ExecuteAsync(empresaId, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Empresa não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<CompanyDto>(result.Value!, true));
    }

    [HttpPost("current/logo")]
    [RequirePermission("empresa.editar")]
    [RequestSizeLimit(MaxLogoUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxLogoUploadBytes)]
    public async Task<IActionResult> UploadLogo(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, "Selecione um arquivo de imagem."));
        }

        await using var stream = file.OpenReadStream();
        var upload = new CompanyLogoUpload(
            stream,
            file.ContentType,
            file.FileName,
            file.Length);

        var result = await _uploadCompanyLogoService.ExecuteAsync(upload, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Empresa não encontrada."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<CompanyDto>(result.Value!, true));
    }
}
