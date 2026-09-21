using AG.CLINICAL.Application.Abstractions.Identity;
using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.FirstAccess;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Core.Employees;

public interface IResendEmployeeFirstAccessInvitationService
{
    Task<Result<EmployeeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ResendEmployeeFirstAccessInvitationService : IResendEmployeeFirstAccessInvitationService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IEmployeeFirstAccessInvitationService _firstAccessInvitationService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ResendEmployeeFirstAccessInvitationService(
        ICurrentTenantContext tenantContext,
        IEmployeesRepository employeesRepository,
        IUsersRepository usersRepository,
        IEmployeeFirstAccessInvitationService firstAccessInvitationService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _employeesRepository = employeesRepository;
        _usersRepository = usersRepository;
        _firstAccessInvitationService = firstAccessInvitationService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EmployeeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var funcionario = await _employeesRepository.GetByIdAndEmpresaIdAsync(
            id,
            empresaId,
            cancellationToken);

        if (funcionario is null)
        {
            return Result<EmployeeDto>.Failure("Funcionário não encontrado.");
        }

        if (!funcionario.Ativo)
        {
            return Result<EmployeeDto>.Failure(
                "Reative o funcionário antes de reenviar o convite de primeiro acesso.");
        }

        var usuario = await _usersRepository.GetByFuncionarioIdAndEmpresaIdAsync(
            funcionario.Id,
            empresaId,
            cancellationToken);

        if (usuario is null)
        {
            return Result<EmployeeDto>.Failure("Usuário de acesso do funcionário não encontrado.");
        }

        if (!usuario.Ativo)
        {
            return Result<EmployeeDto>.Failure(
                "Reative o funcionário antes de reenviar o convite de primeiro acesso.");
        }

        if (!usuario.PendentePrimeiroAcesso)
        {
            return Result<EmployeeDto>.Failure("Este funcionário já concluiu o primeiro acesso.");
        }

        try
        {
            var stageResult = await _firstAccessInvitationService.StageInvitationAsync(
                usuario.Id,
                cancellationToken);

            if (stageResult.IsFailure)
            {
                return Result<EmployeeDto>.Failure(stageResult.Error!);
            }

            await _firstAccessInvitationService.EnqueueInvitationEmailAsync(
                empresaId,
                usuario,
                stageResult.Value!,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userAccessInfo = new EmployeeUserAccessInfo(
                usuario.EmailLogin,
                usuario.PendentePrimeiroAcesso,
                usuario.TipoUsuario == TipoUsuario.Admin);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Funcionario),
                funcionario.Id,
                AcaoAuditoria.Editar,
                dadosNovos: EmployeesAuditSerializer.Serialize(funcionario, empresaId, usuario.EmailLogin),
                cancellationToken: cancellationToken);

            return Result<EmployeeDto>.Success(EmployeesMapper.Map(funcionario, empresaId, userAccessInfo));
        }
        catch (DomainException exception)
        {
            return Result<EmployeeDto>.Failure(exception.Message);
        }
    }
}
