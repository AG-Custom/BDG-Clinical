using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Infra.Data.Services.Permissions;

public sealed class CompanyModuleLicensesProvisioner : ICompanyModuleLicensesProvisioner
{
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly ISystemModulesRepository _systemModulesRepository;
    private readonly IModuleLicensesRepository _moduleLicensesRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyModuleLicensesProvisioner(
        IRepository<Empresa> empresaRepository,
        ISystemModulesRepository systemModulesRepository,
        IModuleLicensesRepository moduleLicensesRepository,
        IUnitOfWork unitOfWork)
    {
        _empresaRepository = empresaRepository;
        _systemModulesRepository = systemModulesRepository;
        _moduleLicensesRepository = moduleLicensesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ProvisionAllModulesAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        var modulos = await _systemModulesRepository.ListActiveAsync(cancellationToken);

        foreach (var modulo in modulos)
        {
            await _moduleLicensesRepository.EnsureActiveLicenseAsync(
                empresaId,
                modulo.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task BackfillAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _empresaRepository.ListAsync(cancellationToken);

        foreach (var empresa in empresas)
        {
            await ProvisionAllModulesAsync(empresa.Id, cancellationToken);
        }
    }
}
