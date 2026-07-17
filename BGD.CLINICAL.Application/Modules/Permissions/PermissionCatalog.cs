namespace BGD.CLINICAL.Application.Modules.Permissions;

public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        // Agenda
        new("agenda.visualizar", "Visualizar agenda", "Agenda", PermissionModuleCodes.Agendamentos, 10),
        new("agenda.visualizar.propria", "Visualizar agenda própria", "Agenda", PermissionModuleCodes.Agendamentos, 11, Parent: "agenda.visualizar"),
        new("agenda.visualizar.equipe", "Visualizar agenda da equipe", "Agenda", PermissionModuleCodes.Agendamentos, 12, Parent: "agenda.visualizar", Implies: ["agenda.visualizar.propria"]),
        new("agendamento.criar", "Criar agendamento", "Agenda", PermissionModuleCodes.Agendamentos, 20),
        new("agendamento.editar", "Editar agendamento", "Agenda", PermissionModuleCodes.Agendamentos, 21),
        new("agendamento.cancelar", "Cancelar agendamento", "Agenda", PermissionModuleCodes.Agendamentos, 22),
        new("agendamento.confirmar", "Confirmar agendamento", "Agenda", PermissionModuleCodes.Agendamentos, 23),
        new("agendamento.concluir", "Concluir agendamento", "Agenda", PermissionModuleCodes.Agendamentos, 24),
        new("agendamento.registrar_falta", "Registrar falta", "Agenda", PermissionModuleCodes.Agendamentos, 25),

        // Paciente
        new("paciente.visualizar", "Visualizar pacientes", "Pacientes", PermissionModuleCodes.Pacientes, 10),
        new("paciente.criar", "Criar paciente", "Pacientes", PermissionModuleCodes.Pacientes, 11),
        new("paciente.editar", "Editar paciente", "Pacientes", PermissionModuleCodes.Pacientes, 12),
        new("paciente.excluir", "Excluir paciente", "Pacientes", PermissionModuleCodes.Pacientes, 13),

        // Pacote
        new("pacote.visualizar", "Visualizar pacotes", "Pacotes", PermissionModuleCodes.Pacientes, 20),
        new("pacote.criar", "Criar pacote", "Pacotes", PermissionModuleCodes.Pacientes, 21),
        new("pacote.editar", "Editar pacote", "Pacotes", PermissionModuleCodes.Pacientes, 22),
        new("pacote.excluir", "Excluir pacote", "Pacotes", PermissionModuleCodes.Pacientes, 23),

        // Compra de pacote
        new("compra_paciente.visualizar", "Visualizar compras de pacote", "Compras de pacote", PermissionModuleCodes.Pacientes, 30),
        new("compra_paciente.criar", "Criar compra de pacote", "Compras de pacote", PermissionModuleCodes.Pacientes, 31),
        new("compra_paciente.cancelar", "Cancelar compra de pacote", "Compras de pacote", PermissionModuleCodes.Pacientes, 32),

        // Sintoma
        new("sintoma.visualizar", "Visualizar sintomas", "Sintomas", PermissionModuleCodes.Pacientes, 10),
        new("sintoma.criar", "Criar sintoma", "Sintomas", PermissionModuleCodes.Pacientes, 11),
        new("sintoma.editar", "Editar sintoma", "Sintomas", PermissionModuleCodes.Pacientes, 12),
        new("sintoma.excluir", "Excluir sintoma", "Sintomas", PermissionModuleCodes.Pacientes, 13),

        // Produto
        new("produto.visualizar", "Visualizar produtos", "Produto", PermissionModuleCodes.Estoque, 10),
        new("produto.criar", "Criar produto", "Produto", PermissionModuleCodes.Estoque, 11),
        new("produto.editar", "Editar produto", "Produto", PermissionModuleCodes.Estoque, 12),
        new("produto.excluir", "Excluir produto", "Produto", PermissionModuleCodes.Estoque, 13),

        // Tipo de produto
        new("tipo_produto.visualizar", "Visualizar tipos de produto", "Tipo de produto", PermissionModuleCodes.Estoque, 10),
        new("tipo_produto.criar", "Criar tipo de produto", "Tipo de produto", PermissionModuleCodes.Estoque, 11),
        new("tipo_produto.editar", "Editar tipo de produto", "Tipo de produto", PermissionModuleCodes.Estoque, 12),
        new("tipo_produto.excluir", "Excluir tipo de produto", "Tipo de produto", PermissionModuleCodes.Estoque, 13),

        // Unidade de medida
        new("unidade_medida.visualizar", "Visualizar unidades de medida", "Unidade de medida", PermissionModuleCodes.Estoque, 10),
        new("unidade_medida.criar", "Criar unidade de medida", "Unidade de medida", PermissionModuleCodes.Estoque, 11),
        new("unidade_medida.editar", "Editar unidade de medida", "Unidade de medida", PermissionModuleCodes.Estoque, 12),
        new("unidade_medida.excluir", "Excluir unidade de medida", "Unidade de medida", PermissionModuleCodes.Estoque, 13),

        // Fornecedor
        new("fornecedor.visualizar", "Visualizar fornecedores", "Fornecedor", PermissionModuleCodes.Estoque, 10),
        new("fornecedor.criar", "Criar fornecedor", "Fornecedor", PermissionModuleCodes.Estoque, 11),
        new("fornecedor.editar", "Editar fornecedor", "Fornecedor", PermissionModuleCodes.Estoque, 12),
        new("fornecedor.excluir", "Excluir fornecedor", "Fornecedor", PermissionModuleCodes.Estoque, 13),

        // Pedido
        new("pedido.visualizar", "Visualizar pedidos", "Pedido", PermissionModuleCodes.Estoque, 10),
        new("pedido.criar", "Criar pedido", "Pedido", PermissionModuleCodes.Estoque, 11),
        new("pedido.editar", "Editar pedido", "Pedido", PermissionModuleCodes.Estoque, 12),
        new("pedido.cancelar", "Cancelar pedido", "Pedido", PermissionModuleCodes.Estoque, 13, Implies: ["pedido.visualizar"]),
        new("pedido.aprovar", "Aprovar pedido", "Pedido", PermissionModuleCodes.Estoque, 14, Implies: ["pedido.visualizar"]),

        // Estoque
        new("estoque.visualizar", "Visualizar estoque", "Estoque", PermissionModuleCodes.Estoque, 10),
        new("estoque.movimentar", "Movimentar estoque", "Estoque", PermissionModuleCodes.Estoque, 11, Implies: ["estoque.visualizar"]),
        new("estoque.ajustar", "Ajustar estoque", "Estoque", PermissionModuleCodes.Estoque, 12, Implies: ["estoque.visualizar"]),
        new("estoque.inventario", "Inventário de estoque", "Estoque", PermissionModuleCodes.Estoque, 13, Implies: ["estoque.visualizar"]),

        // Aplicação
        new("aplicacao.visualizar", "Visualizar aplicações", "Aplicações", PermissionModuleCodes.Aplicacoes, 10),
        new("aplicacao.criar", "Criar aplicação", "Aplicações", PermissionModuleCodes.Aplicacoes, 11),
        new("aplicacao.editar", "Editar aplicação", "Aplicações", PermissionModuleCodes.Aplicacoes, 12),
        new("aplicacao.cancelar", "Cancelar aplicação", "Aplicações", PermissionModuleCodes.Aplicacoes, 13),

        // Procedimento
        new("procedimento.visualizar", "Visualizar procedimentos", "Procedimentos", PermissionModuleCodes.Aplicacoes, 10),
        new("procedimento.criar", "Criar procedimento", "Procedimentos", PermissionModuleCodes.Aplicacoes, 11),
        new("procedimento.editar", "Editar procedimento", "Procedimentos", PermissionModuleCodes.Aplicacoes, 12),

        // Financeiro
        new("financeiro.visualizar", "Visualizar financeiro", "Financeiro", PermissionModuleCodes.Financeiro, 10),
        new("financeiro.receber", "Registrar recebimento", "Financeiro", PermissionModuleCodes.Financeiro, 11, Implies: ["financeiro.visualizar"]),
        new("financeiro.pagar", "Registrar pagamento", "Financeiro", PermissionModuleCodes.Financeiro, 12, Implies: ["financeiro.visualizar"]),
        new("financeiro.estornar", "Estornar lançamento", "Financeiro", PermissionModuleCodes.Financeiro, 13, Implies: ["financeiro.visualizar"]),

        // Core
        new("funcionario.visualizar", "Visualizar funcionários", "Funcionários", PermissionModuleCodes.Core, 10),
        new("funcionario.criar", "Criar funcionário", "Funcionários", PermissionModuleCodes.Core, 11),
        new("funcionario.editar", "Editar funcionário", "Funcionários", PermissionModuleCodes.Core, 12),
        new("unidade.visualizar", "Visualizar unidades", "Unidades", PermissionModuleCodes.Core, 10),
        new("unidade.criar", "Criar unidade", "Unidades", PermissionModuleCodes.Core, 11),
        new("unidade.editar", "Editar unidade", "Unidades", PermissionModuleCodes.Core, 12),
        new("empresa.editar", "Editar empresa", "Empresa", PermissionModuleCodes.Core, 10),
    ];
}
