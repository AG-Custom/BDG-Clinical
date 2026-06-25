using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Core.Employees;

public interface IUpdateEmployeesService
{
    Task<Result<EmployeeDto>> ExecuteAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateEmployeesService : IUpdateEmployeesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IEmployeesRepository _employeesRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeesService(
        ICurrentTenantContext tenantContext,
        IEmployeesRepository employeesRepository,
        IPositionsRepository positionsRepository,
        IUsersRepository usersRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _employeesRepository = employeesRepository;
        _positionsRepository = positionsRepository;
        _usersRepository = usersRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EmployeeDto>> ExecuteAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<EmployeeDto>.Failure("Informe o nome do funcionário.");
        }

        if (!request.LinkToEmpresa && (request.UnidadeIds is null || request.UnidadeIds.Count == 0))
        {
            return Result<EmployeeDto>.Failure("Informe ao menos uma unidade ou vincule o funcionário à empresa.");
        }

        var empresaId = _tenantContext.EmpresaId;
        var funcionario = await _employeesRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (funcionario is null)
        {
            return Result<EmployeeDto>.Failure("Funcionário não encontrado.");
        }

        if (!funcionario.Ativo)
        {
            return Result<EmployeeDto>.Failure("Não é possível editar um funcionário inativo.");
        }

        if (request.CargoId.HasValue
            && !await _positionsRepository.ExistsActiveByIdAndEmpresaIdAsync(request.CargoId.Value, empresaId, cancellationToken))
        {
            return Result<EmployeeDto>.Failure("Cargo não encontrado.");
        }

        var userAccessInfo = await _employeesRepository.GetUserAccessInfoByFuncionarioAndEmpresaAsync(
            funcionario.Id,
            empresaId,
            cancellationToken);

        var usuario = await _usersRepository.GetByFuncionarioIdAndEmpresaIdAsync(
            funcionario.Id,
            empresaId,
            cancellationToken);

        var emailLogin = userAccessInfo?.EmailLogin;
        var dadosAnteriores = EmployeesAuditSerializer.Serialize(funcionario, empresaId, emailLogin);

        try
        {
            var telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            var nome = request.Nome.Trim();

            funcionario.UpdateDetails(nome, telefone, email);

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

            if (usuario is not null)
            {
                usuario.UpdateProfile(nome);
                usuario.SetTipoUsuario(request.IsAdmin ? TipoUsuario.Admin : TipoUsuario.Funcionario);
                _usersRepository.Update(usuario);
            }

            _employeesRepository.Update(funcionario);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            userAccessInfo = await _employeesRepository.GetUserAccessInfoByFuncionarioAndEmpresaAsync(
                funcionario.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Funcionario),
                funcionario.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: EmployeesAuditSerializer.Serialize(funcionario, empresaId, userAccessInfo?.EmailLogin),
                cancellationToken: cancellationToken);

            return Result<EmployeeDto>.Success(EmployeesMapper.Map(funcionario, empresaId, userAccessInfo));
        }
        catch (DomainException exception)
        {
            return Result<EmployeeDto>.Failure(exception.Message);
        }
    }
}
