using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

internal sealed class LoteProdutoConfiguration : IEntityTypeConfiguration<LoteProduto>
{
    public void Configure(EntityTypeBuilder<LoteProduto> builder)
    {
        builder.ToTable("lote_produto");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Codigo).HasMaxLength(80).IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade)
            .WithMany()
            .HasForeignKey(entity => entity.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Produto)
            .WithMany()
            .HasForeignKey(entity => entity.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.UnidadeId, entity.ProdutoId, entity.Codigo })
            .IsUnique();
    }
}
