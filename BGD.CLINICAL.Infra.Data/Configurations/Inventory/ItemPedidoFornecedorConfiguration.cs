using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

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
