using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations;

internal sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresa");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Cnpj).HasMaxLength(20);
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.Property(entity => entity.Logo).HasMaxLength(500);
        builder.Property(entity => entity.CorPrincipal).HasMaxLength(20);
        builder.HasIndex(entity => entity.Cnpj);
    }
}

internal sealed class UnidadeConfiguration : IEntityTypeConfiguration<Unidade>
{
    public void Configure(EntityTypeBuilder<Unidade> builder)
    {
        builder.ToTable("unidade");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Endereco).HasMaxLength(500);
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.Unidades)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome });
    }
}

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.EmailLogin).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.SenhaHash).HasMaxLength(500);
        builder.Property(entity => entity.PendentePrimeiroAcesso).HasDefaultValue(false);
        builder.Property(entity => entity.AuthProvider).HasMaxLength(60).IsRequired();
        builder.Property(entity => entity.GoogleId).HasMaxLength(200);
        builder.Property(entity => entity.TipoUsuario)
            .HasConversion(new TipoUsuarioValueConverter())
            .HasMaxLength(40)
            .IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.Usuarios)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario)
            .WithMany()
            .HasForeignKey(entity => entity.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.EmailLogin }).IsUnique();
    }
}

internal sealed class ConvitePrimeiroAcessoConfiguration : IEntityTypeConfiguration<ConvitePrimeiroAcesso>
{
    public void Configure(EntityTypeBuilder<ConvitePrimeiroAcesso> builder)
    {
        builder.ToTable("convite_primeiro_acesso");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasOne(entity => entity.Usuario)
            .WithMany()
            .HasForeignKey(entity => entity.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.TokenHash).IsUnique();
        builder.HasIndex(entity => new { entity.UsuarioId, entity.UtilizadoEm });
    }
}

internal sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionario");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.HasMany(entity => entity.Vinculos)
            .WithOne(entity => entity.Funcionario)
            .HasForeignKey(entity => entity.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class FuncionarioVinculoConfiguration : IEntityTypeConfiguration<FuncionarioVinculo>
{
    public void Configure(EntityTypeBuilder<FuncionarioVinculo> builder)
    {
        builder.ToTable("funcionario_vinculo");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.FuncionarioVinculos)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade)
            .WithMany()
            .HasForeignKey(entity => entity.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Cargo)
            .WithMany(entity => entity.FuncionarioVinculos)
            .HasForeignKey(entity => entity.CargoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.FuncionarioId, entity.EmpresaId })
            .IsUnique()
            .HasFilter("[empresa_id] IS NOT NULL");
        builder.HasIndex(entity => new { entity.FuncionarioId, entity.UnidadeId })
            .IsUnique()
            .HasFilter("[unidade_id] IS NOT NULL");
        builder.ToTable(table => table.HasCheckConstraint(
            "ck_funcionario_vinculo_empresa_xor_unidade",
            "([empresa_id] IS NOT NULL AND [unidade_id] IS NULL) OR ([empresa_id] IS NULL AND [unidade_id] IS NOT NULL)"));
    }
}

internal sealed class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
        builder.ToTable("cargo");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.Cargos)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}

internal sealed class ModuloSistemaConfiguration : IEntityTypeConfiguration<ModuloSistema>
{
    public void Configure(EntityTypeBuilder<ModuloSistema> builder)
    {
        builder.ToTable("modulo_sistema");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Codigo).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Descricao).HasMaxLength(500);
        builder.HasIndex(entity => entity.Codigo).IsUnique();
    }
}

internal sealed class LicencaModuloConfiguration : IEntityTypeConfiguration<LicencaModulo>
{
    public void Configure(EntityTypeBuilder<LicencaModulo> builder)
    {
        builder.ToTable("licenca_modulo");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Valor).HasPrecision(18, 2);
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.LicencasModulo)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Modulo)
            .WithMany(entity => entity.Licencas)
            .HasForeignKey(entity => entity.ModuloId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.ModuloId }).IsUnique();
    }
}

internal sealed class PermissaoUsuarioConfiguration : IEntityTypeConfiguration<PermissaoUsuario>
{
    public void Configure(EntityTypeBuilder<PermissaoUsuario> builder)
    {
        builder.ToTable("permissao_usuario");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Usuario)
            .WithMany(entity => entity.Permissoes)
            .HasForeignKey(entity => entity.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Modulo)
            .WithMany(entity => entity.PermissoesUsuario)
            .HasForeignKey(entity => entity.ModuloId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.UsuarioId, entity.ModuloId }).IsUnique();
    }
}

internal sealed class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Cpf).HasMaxLength(20);
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cpf })
            .IsUnique()
            .HasFilter("[cpf] IS NOT NULL");
    }
}

internal sealed class SintomaConfiguration : IEntityTypeConfiguration<Sintoma>
{
    public void Configure(EntityTypeBuilder<Sintoma> builder)
    {
        builder.ToTable("sintoma");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}

internal sealed class PacoteConfiguration : IEntityTypeConfiguration<Pacote>
{
    public void Configure(EntityTypeBuilder<Pacote> builder)
    {
        builder.ToTable("pacote");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Descricao).HasMaxLength(1000);
        builder.Property(entity => entity.Valor).HasPrecision(18, 2);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome });
    }
}

internal sealed class ItemPacoteConfiguration : IEntityTypeConfiguration<ItemPacote>
{
    public void Configure(EntityTypeBuilder<ItemPacote> builder)
    {
        builder.ToTable("item_pacote");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.QuantidadeTotal).HasPrecision(18, 4);
        builder.Property(entity => entity.UnidadeMedida).HasMaxLength(30).IsRequired();
        builder.HasOne(entity => entity.Pacote)
            .WithMany(entity => entity.Itens)
            .HasForeignKey(entity => entity.PacoteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.PacoteId, entity.ProdutoId }).IsUnique();
    }
}

internal sealed class CompraPacienteConfiguration : IEntityTypeConfiguration<CompraPaciente>
{
    public void Configure(EntityTypeBuilder<CompraPaciente> builder)
    {
        builder.ToTable("compra_paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany(entity => entity.Compras).HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Pacote).WithMany(entity => entity.Compras).HasForeignKey(entity => entity.PacoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Status });
    }
}

internal sealed class UnidadeMedidaConfiguration : IEntityTypeConfiguration<UnidadeMedida>
{
    public void Configure(EntityTypeBuilder<UnidadeMedida> builder)
    {
        builder.ToTable("unidade_medida");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Sigla).HasMaxLength(30).IsRequired();
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Sigla }).IsUnique();
    }
}

internal sealed class TipoProdutoConfiguration : IEntityTypeConfiguration<TipoProduto>
{
    public void Configure(EntityTypeBuilder<TipoProduto> builder)
    {
        builder.ToTable("tipo_produto");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produto");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Sku).HasMaxLength(50);
        builder.Property(entity => entity.CodigoInterno).HasMaxLength(50);
        builder.Property(entity => entity.CodigoBarras).HasMaxLength(50);
        builder.Property(entity => entity.EstoqueMinimo).HasPrecision(18, 4);
        builder.Property(entity => entity.ControlaEstoque).HasDefaultValue(true);
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.TipoProduto)
            .WithMany(entity => entity.Produtos)
            .HasForeignKey(entity => entity.TipoProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.UnidadeMedida)
            .WithMany(entity => entity.Produtos)
            .HasForeignKey(entity => entity.UnidadeMedidaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Sku })
            .IsUnique()
            .HasFilter("[sku] IS NOT NULL");
        builder.HasIndex(entity => new { entity.EmpresaId, entity.CodigoInterno })
            .IsUnique()
            .HasFilter("[codigo_interno] IS NOT NULL");
    }
}

internal sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedor");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.Property(entity => entity.Cnpj).HasMaxLength(20).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cnpj }).IsUnique();
    }
}

internal sealed class PedidoFornecedorConfiguration : IEntityTypeConfiguration<PedidoFornecedor>
{
    public void Configure(EntityTypeBuilder<PedidoFornecedor> builder)
    {
        builder.ToTable("pedido_fornecedor");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.TipoPedido).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Status)
            .HasConversion(
                status => status.ToApiString(),
                value => StatusPedidoFornecedorExtensions.FromStorage(value))
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(entity => entity.ValorTotal).HasPrecision(18, 2);
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Fornecedor).WithMany(entity => entity.Pedidos).HasForeignKey(entity => entity.FornecedorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(entity => entity.Itens)
            .WithOne(item => item.PedidoFornecedor)
            .HasForeignKey(item => item.PedidoFornecedorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(entity => entity.Itens).HasField("_itens");
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Status });
    }
}

internal sealed class ItemPedidoFornecedorConfiguration : IEntityTypeConfiguration<ItemPedidoFornecedor>
{
    public void Configure(EntityTypeBuilder<ItemPedidoFornecedor> builder)
    {
        builder.ToTable("item_pedido_fornecedor");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Quantidade).HasPrecision(18, 4);
        builder.Property(entity => entity.ValorUnitario).HasPrecision(18, 2);
        builder.Property(entity => entity.ValorTotal).HasPrecision(18, 2);
        builder.HasOne(entity => entity.PedidoFornecedor)
            .WithMany(entity => entity.Itens)
            .HasForeignKey(entity => entity.PedidoFornecedorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("movimentacao_estoque");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Motivo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Quantidade).HasPrecision(18, 4);
        builder.Property(entity => entity.Origem).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AplicacaoPaciente).WithMany(entity => entity.MovimentacoesEstoque).HasForeignKey(entity => entity.AplicacaoPacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.PedidoFornecedor).WithMany().HasForeignKey(entity => entity.PedidoFornecedorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.UnidadeId, entity.ProdutoId, entity.Data });
    }
}

internal sealed class AplicacaoPacienteConfiguration : IEntityTypeConfiguration<AplicacaoPaciente>
{
    public void Configure(EntityTypeBuilder<AplicacaoPaciente> builder)
    {
        builder.ToTable("aplicacao_paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.QuantidadeUtilizada).HasPrecision(18, 4);
        builder.Property(entity => entity.Peso).HasPrecision(10, 3);
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany(entity => entity.Aplicacoes).HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CompraPaciente).WithMany(entity => entity.Aplicacoes).HasForeignKey(entity => entity.CompraPacienteId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cancelada });
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        builder.HasOne(entity => entity.Procedimento).WithMany().HasForeignKey(entity => entity.ProcedimentoId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Agendamento)
            .WithOne(entity => entity.AplicacaoPaciente)
            .HasForeignKey<AplicacaoPaciente>(entity => entity.AgendamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataAplicacao });
    }
}

internal sealed class ProcedimentoConfiguration : IEntityTypeConfiguration<Procedimento>
{
    public void Configure(EntityTypeBuilder<Procedimento> builder)
    {
        builder.ToTable("procedimento");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Observacoes).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ProdutoAplicado)
            .WithMany()
            .HasForeignKey(entity => entity.ProdutoAplicadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}

internal sealed class ItemProcedimentoConfiguration : IEntityTypeConfiguration<ItemProcedimento>
{
    public void Configure(EntityTypeBuilder<ItemProcedimento> builder)
    {
        builder.ToTable("item_procedimento");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Quantidade).HasPrecision(18, 4);
        builder.HasOne(entity => entity.Procedimento)
            .WithMany(entity => entity.Itens)
            .HasForeignKey(entity => entity.ProcedimentoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Produto)
            .WithMany()
            .HasForeignKey(entity => entity.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.ProcedimentoId, entity.ProdutoId }).IsUnique();
    }
}

internal sealed class AplicacaoSintomaConfiguration : IEntityTypeConfiguration<AplicacaoSintoma>
{
    public void Configure(EntityTypeBuilder<AplicacaoSintoma> builder)
    {
        builder.ToTable("aplicacao_sintoma");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.AplicacaoPacienteId, entity.SintomaId }).IsUnique();
        builder.HasOne(entity => entity.AplicacaoPaciente)
            .WithMany(entity => entity.Sintomas)
            .HasForeignKey(entity => entity.AplicacaoPacienteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Sintoma)
            .WithMany(entity => entity.Aplicacoes)
            .HasForeignKey(entity => entity.SintomaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.ToTable("agendamento");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.Property(entity => entity.MotivoCancelamento).HasMaxLength(500);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CompraPaciente).WithMany(entity => entity.Agendamentos).HasForeignKey(entity => entity.CompraPacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CriadoPor).WithMany().HasForeignKey(entity => entity.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CanceladoPor).WithMany().HasForeignKey(entity => entity.CanceladoPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.UnidadeId, entity.DataInicio });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.DataInicio, entity.DataFim });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Status });
    }
}

internal sealed class DisponibilidadeFuncionarioConfiguration : IEntityTypeConfiguration<DisponibilidadeFuncionario>
{
    public void Configure(EntityTypeBuilder<DisponibilidadeFuncionario> builder)
    {
        builder.ToTable("disponibilidade_funcionario");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.DiaSemana).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.UnidadeId, entity.DiaSemana });
    }
}

internal sealed class BloqueioAgendaConfiguration : IEntityTypeConfiguration<BloqueioAgenda>
{
    public void Configure(EntityTypeBuilder<BloqueioAgenda> builder)
    {
        builder.ToTable("bloqueio_agenda");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Motivo).HasMaxLength(500).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CriadoPor).WithMany().HasForeignKey(entity => entity.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.DataInicio, entity.DataFim });
    }
}

internal sealed class ContaGoogleConectadaConfiguration : IEntityTypeConfiguration<ContaGoogleConectada>
{
    public void Configure(EntityTypeBuilder<ContaGoogleConectada> builder)
    {
        builder.ToTable("conta_google_conectada");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.GoogleEmail).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.GoogleAccountId).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.AccessToken).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.RefreshToken).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.EscoposAutorizados).HasMaxLength(1000).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Usuario).WithMany().HasForeignKey(entity => entity.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.GoogleAccountId });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.Ativo });
    }
}

internal sealed class AgendaGoogleConfiguration : IEntityTypeConfiguration<AgendaGoogle>
{
    public void Configure(EntityTypeBuilder<AgendaGoogle> builder)
    {
        builder.ToTable("agenda_google");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.GoogleCalendarId).HasMaxLength(300).IsRequired();
        builder.Property(entity => entity.Nome).HasMaxLength(200).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ContaGoogleConectada)
            .WithMany(entity => entity.Agendas)
            .HasForeignKey(entity => entity.ContaGoogleConectadaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.ContaGoogleConectadaId, entity.GoogleCalendarId }).IsUnique();
    }
}

internal sealed class AgendamentoGoogleSyncConfiguration : IEntityTypeConfiguration<AgendamentoGoogleSync>
{
    public void Configure(EntityTypeBuilder<AgendamentoGoogleSync> builder)
    {
        builder.ToTable("agendamento_google_sync");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.GoogleEventId).HasMaxLength(300);
        builder.Property(entity => entity.StatusSync).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.ErroSync).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Agendamento)
            .WithOne(entity => entity.GoogleSync)
            .HasForeignKey<AgendamentoGoogleSync>(entity => entity.AgendamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AgendaGoogle)
            .WithMany(entity => entity.Sincronizacoes)
            .HasForeignKey(entity => entity.AgendaGoogleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.AgendamentoId).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.StatusSync });
    }
}

internal sealed class FormaPagamentoConfiguration : IEntityTypeConfiguration<FormaPagamento>
{
    public void Configure(EntityTypeBuilder<FormaPagamento> builder)
    {
        builder.ToTable("forma_pagamento");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Tipo).HasMaxLength(80);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}

internal sealed class ContaReceberConfiguration : IEntityTypeConfiguration<ContaReceber>
{
    public void Configure(EntityTypeBuilder<ContaReceber> builder)
    {
        builder.ToTable("conta_receber");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ValorTotal).HasPrecision(18, 2);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CompraPaciente).WithMany().HasForeignKey(entity => entity.CompraPacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Status });
    }
}

internal sealed class PagamentoPacienteConfiguration : IEntityTypeConfiguration<PagamentoPaciente>
{
    public void Configure(EntityTypeBuilder<PagamentoPaciente> builder)
    {
        builder.ToTable("pagamento_paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ValorPago).HasPrecision(18, 2);
        builder.Property(entity => entity.Observacao).HasMaxLength(1000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ContaReceber).WithMany(entity => entity.Pagamentos).HasForeignKey(entity => entity.ContaReceberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.FormaPagamento).WithMany().HasForeignKey(entity => entity.FormaPagamentoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataPagamento });
    }
}

internal sealed class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("log_auditoria");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Entidade).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Acao).HasConversion<string>().HasMaxLength(60).IsRequired();
        builder.Property(entity => entity.DadosAnteriores).HasColumnType("jsonb");
        builder.Property(entity => entity.DadosNovos).HasColumnType("jsonb");
        builder.Property(entity => entity.Ip).HasMaxLength(80);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Usuario).WithMany().HasForeignKey(entity => entity.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Entidade, entity.RegistroId });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.UsuarioId, entity.Data });
    }
}
