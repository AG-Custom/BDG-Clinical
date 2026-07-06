using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Context;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<FuncionarioVinculo> FuncionarioVinculos => Set<FuncionarioVinculo>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<ModuloSistema> ModulosSistema => Set<ModuloSistema>();
    public DbSet<LicencaModulo> LicencasModulo => Set<LicencaModulo>();
    public DbSet<PermissaoSistema> PermissoesSistema => Set<PermissaoSistema>();
    public DbSet<CargoPermissaoItem> CargoPermissaoItens => Set<CargoPermissaoItem>();
    public DbSet<UsuarioPermissaoOverride> UsuarioPermissoesOverride => Set<UsuarioPermissaoOverride>();
    public DbSet<ConvitePrimeiroAcesso> ConvitesPrimeiroAcesso => Set<ConvitePrimeiroAcesso>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<PacienteUnidade> PacientesUnidade => Set<PacienteUnidade>();
    public DbSet<Sintoma> Sintomas => Set<Sintoma>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<UnidadeMedida> UnidadesMedida => Set<UnidadeMedida>();
    public DbSet<TipoProduto> TiposProduto => Set<TipoProduto>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<PedidoFornecedor> PedidosFornecedor => Set<PedidoFornecedor>();
    public DbSet<ItemPedidoFornecedor> ItensPedidoFornecedor => Set<ItemPedidoFornecedor>();
    public DbSet<AnexoPedidoFornecedor> AnexosPedidoFornecedor => Set<AnexoPedidoFornecedor>();
    public DbSet<Procedimento> Procedimentos => Set<Procedimento>();
    public DbSet<ItemProcedimento> ItensProcedimento => Set<ItemProcedimento>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<AplicacaoPaciente> AplicacoesPaciente => Set<AplicacaoPaciente>();
    public DbSet<AplicacaoSintoma> AplicacoesSintomas => Set<AplicacaoSintoma>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<AgendamentoProcedimento> AgendamentosProcedimento => Set<AgendamentoProcedimento>();
    public DbSet<HorarioFuncionamentoUnidade> HorariosFuncionamentoUnidade => Set<HorarioFuncionamentoUnidade>();
    public DbSet<DisponibilidadeFuncionario> DisponibilidadesFuncionario => Set<DisponibilidadeFuncionario>();
    public DbSet<BloqueioAgenda> BloqueiosAgenda => Set<BloqueioAgenda>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<OutputMessageEmail> OutputMessagesEmail => Set<OutputMessageEmail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Pacote>();
        modelBuilder.Ignore<ItemPacote>();
        modelBuilder.Ignore<CompraPaciente>();
        modelBuilder.Ignore<FormaPagamento>();
        modelBuilder.Ignore<ContaReceber>();
        modelBuilder.Ignore<PagamentoPaciente>();
        modelBuilder.Ignore<ContaGoogleConectada>();
        modelBuilder.Ignore<AgendaGoogle>();
        modelBuilder.Ignore<AgendamentoGoogleSync>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplySnakeCaseNaming(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySnakeCaseNaming(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName() ?? string.Empty));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? string.Empty));
            }
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var chars = new List<char>(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (char.IsUpper(current))
            {
                if (i > 0 && value[i - 1] != '_' && !char.IsUpper(value[i - 1]))
                {
                    chars.Add('_');
                }

                chars.Add(char.ToLowerInvariant(current));
                continue;
            }

            chars.Add(current == '-' ? '_' : current);
        }

        return new string(chars.ToArray());
    }
}
