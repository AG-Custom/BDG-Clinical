using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Core.Employees;
using BGD.CLINICAL.Application.Modules.Dtos;
using BGD.CLINICAL.Application.Modules.EmployeePermissions;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.Employee;

[ApiController]
[Authorize]
[Route("api/employees")]
public sealed class EmployeeController : ControllerBase
{
    private readonly ICreateEmployeesService _createEmployeesService;
    private readonly IListEmployeesService _listEmployeesService;
    private readonly IGetEmployeesService _getEmployeesService;
    private readonly IUpdateEmployeesService _updateEmployeesService;
    private readonly IDeactivateEmployeesService _deactivateEmployeesService;
    private readonly IReactivateEmployeesService _reactivateEmployeesService;
    private readonly IGetEmployeePermissionsService _getEmployeePermissionsService;
    private readonly IUpdateEmployeePermissionsService _updateEmployeePermissionsService;

    public EmployeeController(
        ICreateEmployeesService createEmployeesService,
        IListEmployeesService listEmployeesService,
        IGetEmployeesService getEmployeesService,
        IUpdateEmployeesService updateEmployeesService,
        IDeactivateEmployeesService deactivateEmployeesService,
        IReactivateEmployeesService reactivateEmployeesService,
        IGetEmployeePermissionsService getEmployeePermissionsService,
        IUpdateEmployeePermissionsService updateEmployeePermissionsService)
    {
        _createEmployeesService = createEmployeesService;
        _listEmployeesService = listEmployeesService;
        _getEmployeesService = getEmployeesService;
        _updateEmployeesService = updateEmployeesService;
        _deactivateEmployeesService = deactivateEmployeesService;
        _reactivateEmployeesService = reactivateEmployeesService;
        _getEmployeePermissionsService = getEmployeePermissionsService;
        _updateEmployeePermissionsService = updateEmployeePermissionsService;
    }

    [HttpGet]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Employees)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? unidadeId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _listEmployeesService.ExecuteAsync(unidadeId, includeInactive, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<EmployeeDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequireAnyPermissionFrom(AuxiliaryPermissionSet.Employees)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getEmployeesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<EmployeeDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("funcionario.criar")]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createEmployeesService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<EmployeeDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("funcionario.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateEmployeesService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Funcionário não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<EmployeeDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("funcionario.editar")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deactivateEmployeesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Funcionário não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<EmployeeDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/reactivate")]
    [RequirePermission("funcionario.editar")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reactivateEmployeesService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Funcionário não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<EmployeeDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/permissions")]
    [RequirePermission("funcionario.editar")]
    public async Task<IActionResult> GetPermissions(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getEmployeePermissionsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Funcionário não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<EmployeePermissionsDto>(result.Value!, true));
    }

    [HttpPut("{id:guid}/permissions")]
    [RequirePermission("funcionario.editar")]
    public async Task<IActionResult> UpdatePermissions(
        Guid id,
        [FromBody] UpdateEmployeePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateEmployeePermissionsService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error is "Funcionário não encontrado." or "Perfil de permissão não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<EmployeePermissionsDto>(result.Value!, true));
    }
}
