using BGD.CLINICAL.Application.Abstractions.Identity;
using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.FirstAccess;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Core.Employees;

public interface ICreateEmployeesService
{
    Task<Result<EmployeeDto>> ExecuteAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateEmployeesService : ICreateEmployeesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IProvisionEmployeeUsersService _provisionEmployeeUsersService;
    private readonly IEmployeeFirstAccessInvitationService _firstAccessInvitationService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeesService(
        ICurrentTenantContext tenantContext,
        IEmployeesRepository employeesRepository,
        IPositionsRepository positionsRepository,
        IUsersRepository usersRepository,
        IRepository<Empresa> empresaRepository,
        IProvisionEmployeeUsersService provisionEmployeeUsersService,
        IEmployeeFirstAccessInvitationService firstAccessInvitationService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _employeesRepository = employeesRepository;
        _positionsRepository = positionsRepository;
        _usersRepository = usersRepository;
        _empresaRepository = empresaRepository;
        _provisionEmployeeUsersService = provisionEmployeeUsersService;
        _firstAccessInvitationService = firstAccessInvitationService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EmployeeDto>> ExecuteAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return Result<EmployeeDto>.Failure(validationError);
        }

        var empresaId = _tenantContext.EmpresaId;
        var emailLogin = IdentityValidation.NormalizeEmail(request.EmailLogin);

        if (await _usersRepository.ExistsActiveEmailLoginByEmpresaAsync(empresaId, emailLogin, cancellationToken))
        {
            return Result<EmployeeDto>.Failure("Já existe um usuário com este e-mail nesta empresa.");
        }

        if (request.CargoId.HasValue
            && !await _positionsRepository.ExistsActiveByIdAndEmpresaIdAsync(request.CargoId.Value, empresaId, cancellationToken))
        {
            return Result<EmployeeDto>.Failure("Cargo não encontrado.");
        }

        var empresa = await _empresaRepository.GetByIdAsync(empresaId, cancellationToken);
        if (empresa is null)
        {
            return Result<EmployeeDto>.Failure("Empresa não encontrada.");
        }

        try
        {
            var telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            var funcionario = Funcionario.Create(request.Nome.Trim(), telefone, email);

            var linkResult = await EmployeesLinkSync.ApplyAsync(
                funcionario,
                empresaId,
                request.LinkToEmpresa,
                request.UnidadeIds,
                request.CargoId,
                request.FlagAplicador,
                _employeesRepository,
                cancellationToken);

            if (linkResult.IsFailure)
            {
                return Result<EmployeeDto>.Failure(linkResult.Error!);
            }

            await _employeesRepository.AddAsync(funcionario, cancellationToken);
            var usuario = await _provisionEmployeeUsersService.ProvisionAsync(
                empresaId,
                funcionario.Id,
                funcionario.Nome,
                emailLogin,
                request.IsAdmin,
                cancellationToken);

            var stageResult = await _firstAccessInvitationService.StageInvitationAsync(
                usuario.Id,
                cancellationToken);

            if (stageResult.IsFailure)
            {
                return Result<EmployeeDto>.Failure(stageResult.Error!);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var emailResult = await _firstAccessInvitationService.SendInvitationEmailAsync(
                usuario,
                empresa.Nome,
                stageResult.Value!,
                cancellationToken);

            if (emailResult.IsFailure)
            {
                return Result<EmployeeDto>.Failure(emailResult.Error!);
            }

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Funcionario),
                funcionario.Id,
                AcaoAuditoria.Criar,
                dadosNovos: EmployeesAuditSerializer.Serialize(funcionario, empresaId, emailLogin),
                cancellationToken: cancellationToken);

            return Result<EmployeeDto>.Success(EmployeesMapper.Map(
                funcionario,
                empresaId,
                new EmployeeUserAccessInfo(emailLogin, PendentePrimeiroAcesso: true, request.IsAdmin)));
        }
        catch (DomainException exception)
        {
            return Result<EmployeeDto>.Failure(exception.Message);
        }
    }

    private static string? ValidateRequest(CreateEmployeeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return "Informe o nome do funcionário.";
        }

        if (!IdentityValidation.IsValidEmail(request.EmailLogin))
        {
            return "Informe um e-mail de login válido.";
        }

        if (!request.LinkToEmpresa && (request.UnidadeIds is null || request.UnidadeIds.Count == 0))
        {
            return "Informe ao menos uma unidade ou vincule o funcionário à empresa.";
        }

        return null;
    }
}
