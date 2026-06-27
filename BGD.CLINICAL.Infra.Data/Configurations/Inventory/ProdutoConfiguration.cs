using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

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
