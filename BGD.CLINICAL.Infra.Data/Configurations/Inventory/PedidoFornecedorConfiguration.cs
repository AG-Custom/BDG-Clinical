using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

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
