using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

internal sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("movimentacao_estoque");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Motivo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Quantidade).HasPrecision(18, 4);
        builder.Property(entity => entity.QuantidadeEmbalagem).HasPrecision(18, 4);
        builder.Property(entity => entity.Origem).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.LoteProduto).WithMany().HasForeignKey(entity => entity.LoteProdutoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AplicacaoPaciente).WithMany(entity => entity.MovimentacoesEstoque).HasForeignKey(entity => entity.AplicacaoPacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.PedidoFornecedor).WithMany().HasForeignKey(entity => entity.PedidoFornecedorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.UnidadeId, entity.ProdutoId, entity.Data });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.LoteProdutoId });
    }
}
