using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.Authentications;
using AG.CLINICAL.Application.Identity.Dtos;
using AG.CLINICAL.Application.Identity.FirstAccess;
using AG.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.CLINICAL.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ILoginUsersService _loginUsersService;
    private readonly ISwitchCompanyService _switchCompanyService;
    private readonly IRegisterCompaniesService _registerCompaniesService;
    private readonly IGetAuthenticatedUsersService _getAuthenticatedUsersService;
    private readonly IValidateFirstAccessEmailsService _validateFirstAccessEmailsService;
    private readonly ICompleteFirstAccessService _completeFirstAccessService;

    public AuthController(
        ILoginUsersService loginUsersService,
        ISwitchCompanyService switchCompanyService,
        IRegisterCompaniesService registerCompaniesService,
        IGetAuthenticatedUsersService getAuthenticatedUsersService,
        IValidateFirstAccessEmailsService validateFirstAccessEmailsService,
        ICompleteFirstAccessService completeFirstAccessService)
    {
        _loginUsersService = loginUsersService;
        _switchCompanyService = switchCompanyService;
        _registerCompaniesService = registerCompaniesService;
        _getAuthenticatedUsersService = getAuthenticatedUsersService;
        _validateFirstAccessEmailsService = validateFirstAccessEmailsService;
        _completeFirstAccessService = completeFirstAccessService;
    }

    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar(
        [FromBody] RegisterCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registerCompaniesService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AuthResponse>(result.Value!, true));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _loginUsersService.ExecuteAsync(request, ip, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<LoginResponse>(result.Value!, true));
    }

    [HttpPost("switch-company")]
    [Authorize]
    public async Task<IActionResult> SwitchCompany(
        [FromBody] SwitchCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _switchCompanyService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AuthResponse>(result.Value!, true));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _getAuthenticatedUsersService.ExecuteAsync(User, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AuthenticatedUserDto>(result.Value!, true));
    }

    [HttpPost("primeiro-acesso/validar-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateFirstAccessEmail(
        [FromBody] ValidateFirstAccessEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _validateFirstAccessEmailsService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<ValidateFirstAccessEmailResponse>(result.Value!, true));
    }

    [HttpPost("primeiro-acesso/concluir")]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteFirstAccess(
        [FromBody] CompleteFirstAccessRequest request,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _completeFirstAccessService.ExecuteAsync(request, ip, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<AuthResponse>(result.Value!, true));
    }
}
