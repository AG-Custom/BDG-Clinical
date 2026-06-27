using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Applications;

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
