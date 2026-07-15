using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Packages;

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
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Produto)
            .WithMany()
            .HasForeignKey(entity => entity.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.PacoteId, entity.ProdutoId }).IsUnique();
    }
}
