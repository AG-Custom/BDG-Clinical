namespace BGD.CLINICAL.WebApi.Authorization;

/// <summary>
/// Permissões alternativas para leituras auxiliares (dropdown/filtro) usadas fora do CRUD principal do recurso.
/// Mapeamento: .cursor/mapeamento-permissoes-tela.md
/// </summary>
internal static class AuxiliaryPermissionAlternates
{
    public static readonly string[] Units =
    [
        "unidade.visualizar",
        "agenda.visualizar",
        "paciente.visualizar",
        "paciente.criar",
        "funcionario.visualizar",
        "pedido.visualizar",
        "estoque.visualizar",
        "agendamento.criar",
        "aplicacao.visualizar"
    ];

    public static readonly string[] Employees =
    [
        "funcionario.visualizar",
        "agenda.visualizar",
        "agendamento.criar",
        "agendamento.editar",
        "aplicacao.visualizar"
    ];

    public static readonly string[] OperatingHours =
    [
        "unidade.visualizar",
        "agenda.visualizar"
    ];

    public static readonly string[] Patients =
    [
        "paciente.visualizar",
        "agendamento.criar",
        "aplicacao.visualizar"
    ];

    public static readonly string[] Procedures =
    [
        "procedimento.visualizar",
        "agendamento.criar",
        "aplicacao.visualizar"
    ];

    public static readonly string[] Positions =
    [
        "funcionario.visualizar",
        "aplicacao.visualizar"
    ];

    public static readonly string[] ProductTypes =
    [
        "tipo_produto.visualizar",
        "produto.visualizar"
    ];

    public static readonly string[] MeasurementUnits =
    [
        "unidade_medida.visualizar",
        "produto.criar",
        "produto.editar"
    ];

    public static readonly string[] Products =
    [
        "produto.visualizar",
        "pedido.criar",
        "pedido.editar",
        "procedimento.criar",
        "procedimento.editar",
        "estoque.visualizar",
        "estoque.movimentar",
        "aplicacao.criar",
        "aplicacao.editar"
    ];

    public static readonly string[] Suppliers =
    [
        "fornecedor.visualizar",
        "pedido.visualizar"
    ];

    public static readonly string[] Symptoms =
    [
        "sintoma.visualizar",
        "aplicacao.criar",
        "aplicacao.editar"
    ];

    public static readonly string[] Packages =
    [
        "pacote.visualizar",
        "compra_paciente.criar",
        "aplicacao.criar",
        "agendamento.criar"
    ];

    public static readonly string[] PatientPurchases =
    [
        "compra_paciente.visualizar",
        "aplicacao.criar",
        "aplicacao.editar",
        "agendamento.criar",
        "agendamento.editar"
    ];
}
