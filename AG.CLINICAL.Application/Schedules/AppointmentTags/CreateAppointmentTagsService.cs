using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Schedules.AppointmentTags;

public interface ICreateAppointmentTagsService
{
    Task<Result<AppointmentTagCatalogDto>> ExecuteAsync(
        CreateAppointmentTagRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateAppointmentTagsService : ICreateAppointmentTagsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentTagsRepository _appointmentTagsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAppointmentTagsService(
        ICurrentTenantContext tenantContext,
        IAppointmentTagsRepository appointmentTagsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _appointmentTagsRepository = appointmentTagsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentTagCatalogDto>> ExecuteAsync(
        CreateAppointmentTagRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<AppointmentTagCatalogDto>.Failure("Informe o nome da tag.");
        }

        if (string.IsNullOrWhiteSpace(request.Cor))
        {
            return Result<AppointmentTagCatalogDto>.Failure("Informe a cor da tag.");
        }

        var nome = request.Nome.Trim();
        var empresaId = _tenantContext.EmpresaId;

        if (await _appointmentTagsRepository.ExistsByNomeAsync(empresaId, nome, null, cancellationToken))
        {
            return Result<AppointmentTagCatalogDto>.Failure("Já existe uma tag com este nome.");
        }

        try
        {
            var tag = TagAgendamento.Create(empresaId, nome, request.Cor);

            await _appointmentTagsRepository.AddAsync(tag, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(TagAgendamento),
                tag.Id,
                AcaoAuditoria.Criar,
                dadosNovos: AppointmentTagsAuditSerializer.Serialize(tag),
                cancellationToken: cancellationToken);

            return Result<AppointmentTagCatalogDto>.Success(AppointmentTagsMapper.Map(tag));
        }
        catch (DomainException exception)
        {
            return Result<AppointmentTagCatalogDto>.Failure(exception.Message);
        }
    }
}
