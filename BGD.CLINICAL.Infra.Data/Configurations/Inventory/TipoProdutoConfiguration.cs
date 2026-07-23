using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

internal sealed class TipoProdutoConfiguration : IEntityTypeConfiguration<TipoProduto>
{
    public void Configure(EntityTypeBuilder<TipoProduto> builder)
    {
        builder.ToTable("tipo_produto");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Codigo).HasMaxLength(40);
        builder.Ignore(entity => entity.EhTipoSistema);
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Codigo })
            .IsUnique()
            .HasFilter("[codigo] IS NOT NULL");
    }
}
