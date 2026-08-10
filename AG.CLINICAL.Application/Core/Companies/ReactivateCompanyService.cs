using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Core.Companies;

public interface IReactivateCompanyService
{
    Task<Result<CompanyDto>> ExecuteAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default);
}

public sealed class ReactivateCompanyService : IReactivateCompanyService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICompaniesRepository _companiesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateCompanyService(
        ICurrentTenantContext tenantContext,
        ICompaniesRepository companiesRepository,
        IUsersRepository usersRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _companiesRepository = companiesRepository;
        _usersRepository = usersRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CompanyDto>> ExecuteAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var usuarioAtual = await _usersRepository.GetByIdAsync(_tenantContext.UsuarioId, cancellationToken);

        if (usuarioAtual is null || !usuarioAtual.Ativo)
        {
            return Result<CompanyDto>.Failure("Usuário não autenticado.");
        }

        if (usuarioAtual.TipoUsuario != TipoUsuario.Admin)
        {
            return Result<CompanyDto>.Failure("Somente administradores podem reativar a clínica.");
        }

        var usuarioDestino = await _usersRepository.GetByEmailLoginAndEmpresaIdAsync(
            usuarioAtual.EmailLogin,
            empresaId,
            cancellationToken);

        if (usuarioDestino is null || !usuarioDestino.Ativo || usuarioDestino.TipoUsuario != TipoUsuario.Admin)
        {
            return Result<CompanyDto>.Failure("Você não tem permissão para reativar esta clínica.");
        }

        var empresa = await _companiesRepository.GetByIdAsync(empresaId, cancellationToken);

        if (empresa is null)
        {
            return Result<CompanyDto>.Failure("Empresa não encontrada.");
        }

        if (empresa.Ativo)
        {
            return Result<CompanyDto>.Failure("Esta clínica já está ativa.");
        }

        var dadosAnteriores = CompaniesAuditSerializer.Serialize(empresa);

        empresa.Reactivate();
        _companiesRepository.Update(empresa);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Domain.Entities.Empresa),
            empresa.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: CompaniesAuditSerializer.Serialize(empresa),
            cancellationToken: cancellationToken);

        return Result<CompanyDto>.Success(CompaniesMapper.Map(empresa));
    }
}
