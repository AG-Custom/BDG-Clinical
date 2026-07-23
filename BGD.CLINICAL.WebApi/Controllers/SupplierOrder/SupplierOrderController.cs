using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Application.Inventory.SupplierOrders;
using BGD.CLINICAL.WebApi.Authorization;
using BGD.CLINICAL.WebApi.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGD.CLINICAL.WebApi.Controllers.SupplierOrder;

[ApiController]
[Authorize]
[Route("api/supplier-orders")]
public sealed class SupplierOrderController : ControllerBase
{
    private const long MaxAttachmentUploadBytes = 10 * 1024 * 1024;

    private readonly ICreateSupplierOrdersService _createSupplierOrdersService;
    private readonly IListSupplierOrdersService _listSupplierOrdersService;
    private readonly IGetSupplierOrdersService _getSupplierOrdersService;
    private readonly IUpdateSupplierOrdersService _updateSupplierOrdersService;
    private readonly ICancelSupplierOrdersService _cancelSupplierOrdersService;
    private readonly IReceiveSupplierOrdersService _receiveSupplierOrdersService;
    private readonly IUploadSupplierOrderAttachmentService _uploadSupplierOrderAttachmentService;
    private readonly IListSupplierOrderAttachmentsService _listSupplierOrderAttachmentsService;
    private readonly IDeleteSupplierOrderAttachmentService _deleteSupplierOrderAttachmentService;

    public SupplierOrderController(
        ICreateSupplierOrdersService createSupplierOrdersService,
        IListSupplierOrdersService listSupplierOrdersService,
        IGetSupplierOrdersService getSupplierOrdersService,
        IUpdateSupplierOrdersService updateSupplierOrdersService,
        ICancelSupplierOrdersService cancelSupplierOrdersService,
        IReceiveSupplierOrdersService receiveSupplierOrdersService,
        IUploadSupplierOrderAttachmentService uploadSupplierOrderAttachmentService,
        IListSupplierOrderAttachmentsService listSupplierOrderAttachmentsService,
        IDeleteSupplierOrderAttachmentService deleteSupplierOrderAttachmentService)
    {
        _createSupplierOrdersService = createSupplierOrdersService;
        _listSupplierOrdersService = listSupplierOrdersService;
        _getSupplierOrdersService = getSupplierOrdersService;
        _updateSupplierOrdersService = updateSupplierOrdersService;
        _cancelSupplierOrdersService = cancelSupplierOrdersService;
        _receiveSupplierOrdersService = receiveSupplierOrdersService;
        _uploadSupplierOrderAttachmentService = uploadSupplierOrderAttachmentService;
        _listSupplierOrderAttachmentsService = listSupplierOrderAttachmentsService;
        _deleteSupplierOrderAttachmentService = deleteSupplierOrderAttachmentService;
    }

    [HttpGet]
    [RequirePermission("pedido.visualizar")]
    public async Task<IActionResult> List(
        [FromQuery] string? status = null,
        [FromQuery] Guid? fornecedorId = null,
        [FromQuery] Guid? unidadeId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _listSupplierOrdersService.ExecuteAsync(status, fornecedorId, unidadeId, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<SupplierOrderDto>>(result.Value!, true));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("pedido.visualizar")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getSupplierOrdersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }

    [HttpPost]
    [RequirePermission("pedido.criar")]
    [RequestSizeLimit(MaxAttachmentUploadBytes * 10)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxAttachmentUploadBytes * 10)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        CreateSupplierOrderRequest? request;
        IFormFileCollection? files = null;

        if (Request.HasFormContentType)
        {
            var data = Request.Form["data"].ToString();
            if (string.IsNullOrWhiteSpace(data))
            {
                return BadRequest(new ApiResponse<object?>(null!, false, "Informe os dados do pedido no campo data."));
            }

            try
            {
                request = System.Text.Json.JsonSerializer.Deserialize<CreateSupplierOrderRequest>(
                    data,
                    SupplierOrderControllerJson.Options);
            }
            catch (System.Text.Json.JsonException)
            {
                return BadRequest(new ApiResponse<object?>(null!, false, "JSON do pedido inválido."));
            }

            files = Request.Form.Files;
        }
        else
        {
            request = await Request.ReadFromJsonAsync<CreateSupplierOrderRequest>(
                SupplierOrderControllerJson.Options,
                cancellationToken);
        }

        if (request is null)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, "Informe os dados do pedido."));
        }

        var result = await _createSupplierOrdersService.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, result.Error));
        }

        if (files is { Count: > 0 })
        {
            var pedidoId = result.Value!.Id;

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                await using var stream = file.OpenReadStream();
                var upload = new SupplierOrderAttachmentUpload(
                    stream,
                    file.ContentType,
                    file.FileName,
                    file.Length);

                var uploadResult = await _uploadSupplierOrderAttachmentService.ExecuteAsync(
                    pedidoId,
                    upload,
                    cancellationToken);

                if (uploadResult.IsFailure)
                {
                    return BadRequest(new ApiResponse<object?>(null!, false, uploadResult.Error));
                }
            }

            var refreshed = await _getSupplierOrdersService.ExecuteAsync(pedidoId, cancellationToken);
            if (refreshed.IsSuccess)
            {
                result = refreshed;
            }
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            new ApiResponse<SupplierOrderDto>(result.Value, true));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("pedido.editar")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateSupplierOrdersService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/cancel")]
    [RequirePermission("pedido.cancelar")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _cancelSupplierOrdersService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }

    [HttpPatch("{id:guid}/receive")]
    [RequirePermission("pedido.aprovar")]
    public async Task<IActionResult> Receive(
        Guid id,
        [FromBody] ReceiveSupplierOrderRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _receiveSupplierOrdersService.ExecuteAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<SupplierOrderDto>(result.Value!, true));
    }

    [HttpGet("{id:guid}/attachments")]
    [RequirePermission("pedido.visualizar")]
    public async Task<IActionResult> ListAttachments(Guid id, CancellationToken cancellationToken)
    {
        var result = await _listSupplierOrderAttachmentsService.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<IReadOnlyList<SupplierOrderAttachmentDto>>(result.Value!, true));
    }

    [HttpPost("{id:guid}/attachments")]
    [RequirePermission("pedido.editar")]
    [RequestSizeLimit(MaxAttachmentUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxAttachmentUploadBytes)]
    public async Task<IActionResult> UploadAttachment(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new ApiResponse<object?>(null!, false, "Selecione um arquivo."));
        }

        await using var stream = file.OpenReadStream();
        var upload = new SupplierOrderAttachmentUpload(
            stream,
            file.ContentType,
            file.FileName,
            file.Length);

        var result = await _uploadSupplierOrderAttachmentService.ExecuteAsync(id, upload, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Pedido não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return CreatedAtAction(
            nameof(ListAttachments),
            new { id },
            new ApiResponse<SupplierOrderAttachmentDto>(result.Value!, true));
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [RequirePermission("pedido.editar")]
    public async Task<IActionResult> DeleteAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await _deleteSupplierOrderAttachmentService.ExecuteAsync(id, attachmentId, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error == "Anexo não encontrado."
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object?>(null!, false, result.Error));
        }

        return Ok(new ApiResponse<object?>(null!, true));
    }
}
