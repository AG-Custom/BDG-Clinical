using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

internal sealed class AnexoPedidoFornecedorConfiguration : IEntityTypeConfiguration<AnexoPedidoFornecedor>
{
    public void Configure(EntityTypeBuilder<AnexoPedidoFornecedor> builder)
    {
        builder.ToTable("anexo_pedido_fornecedor");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.NomeArquivo).HasMaxLength(260).IsRequired();
        builder.Property(entity => entity.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.ObjectKey).HasMaxLength(500).IsRequired();
        builder.HasOne(entity => entity.PedidoFornecedor)
            .WithMany(pedido => pedido.Anexos)
            .HasForeignKey(entity => entity.PedidoFornecedorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(entity => new { entity.PedidoFornecedorId, entity.EmpresaId });
    }
}
