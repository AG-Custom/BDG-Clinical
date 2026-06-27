using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

internal sealed class UnidadeMedidaConfiguration : IEntityTypeConfiguration<UnidadeMedida>
{
    public void Configure(EntityTypeBuilder<UnidadeMedida> builder)
    {
        builder.ToTable("unidade_medida");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Sigla).HasMaxLength(30).IsRequired();
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Sigla }).IsUnique();
    }
}
