using BGD.CLINICAL.Application.Modules.Permissions;

namespace BGD.CLINICAL.Application.Modules;

public static class SystemModulesCatalog
{
    public static IReadOnlyList<SystemModuleDefinition> All { get; } =
    [
        new(ModuleCodes.Schedules, "Agendamentos", "Agenda e agendamentos"),
        new(PermissionModuleCodes.Pacientes, "Pacientes", "Cadastro e gestão de pacientes"),
        new(ModuleCodes.Inventory, "Estoque", "Produtos, fornecedores e movimentações"),
        new(ModuleCodes.Applications, "Aplicações", "Aplicações de procedimentos em pacientes"),
        new(ModuleCodes.Financial, "Financeiro", "Contas a receber e pagamentos"),
        new(ModuleCodes.Reports, "Relatórios", "Relatórios e indicadores"),
    ];
}
