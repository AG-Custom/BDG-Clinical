using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations;

internal sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
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
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Email).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Senha).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.TipoUsuario).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.Usuarios)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario)
            .WithMany()
            .HasForeignKey(entity => entity.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Email }).IsUnique();
    }
}

internal sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.Funcionarios)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Cargo)
            .WithMany(entity => entity.Funcionarios)
            .HasForeignKey(entity => entity.CargoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome });
    }
}

internal sealed class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
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
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
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
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Cpf).HasMaxLength(20);
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Observacoes).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cpf });
    }
}

internal sealed class SintomaConfiguration : IEntityTypeConfiguration<Sintoma>
{
    public void Configure(EntityTypeBuilder<Sintoma> builder)
    {
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
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany(entity => entity.Compras).HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Pacote).WithMany(entity => entity.Compras).HasForeignKey(entity => entity.PacoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Status });
    }
}

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.UnidadeMedida).HasMaxLength(30).IsRequired();
        builder.Property(entity => entity.EstoqueMinimo).HasPrecision(18, 4);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome });
    }
}

internal sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.Property(entity => entity.Cnpj).HasMaxLength(20);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cnpj });
    }
}

internal sealed class PedidoFornecedorConfiguration : IEntityTypeConfiguration<PedidoFornecedor>
{
    public void Configure(EntityTypeBuilder<PedidoFornecedor> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.TipoPedido).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Fornecedor).WithMany(entity => entity.Pedidos).HasForeignKey(entity => entity.FornecedorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Status });
    }
}

internal sealed class ItemPedidoFornecedorConfiguration : IEntityTypeConfiguration<ItemPedidoFornecedor>
{
    public void Configure(EntityTypeBuilder<ItemPedidoFornecedor> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Quantidade).HasPrecision(18, 4);
        builder.Property(entity => entity.ValorUnitario).HasPrecision(18, 2);
        builder.HasOne(entity => entity.PedidoFornecedor)
            .WithMany(entity => entity.Itens)
            .HasForeignKey(entity => entity.PedidoFornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Quantidade).HasPrecision(18, 4);
        builder.Property(entity => entity.Origem).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Observacoes).HasMaxLength(2000);
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
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.QuantidadeUtilizada).HasPrecision(18, 4);
        builder.Property(entity => entity.Peso).HasPrecision(10, 3);
        builder.Property(entity => entity.Observacoes).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany(entity => entity.Aplicacoes).HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CompraPaciente).WithMany(entity => entity.Aplicacoes).HasForeignKey(entity => entity.CompraPacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataAplicacao });
    }
}

internal sealed class AplicacaoSintomaConfiguration : IEntityTypeConfiguration<AplicacaoSintoma>
{
    public void Configure(EntityTypeBuilder<AplicacaoSintoma> builder)
    {
        builder.HasKey(entity => new { entity.AplicacaoPacienteId, entity.SintomaId });
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

internal sealed class FormaPagamentoConfiguration : IEntityTypeConfiguration<FormaPagamento>
{
    public void Configure(EntityTypeBuilder<FormaPagamento> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}

internal sealed class ContaReceberConfiguration : IEntityTypeConfiguration<ContaReceber>
{
    public void Configure(EntityTypeBuilder<ContaReceber> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Valor).HasPrecision(18, 2);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
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
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Valor).HasPrecision(18, 2);
        builder.Property(entity => entity.Observacoes).HasMaxLength(1000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ContaReceber).WithMany(entity => entity.Pagamentos).HasForeignKey(entity => entity.ContaReceberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.FormaPagamento).WithMany().HasForeignKey(entity => entity.FormaPagamentoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataPagamento });
    }
}
