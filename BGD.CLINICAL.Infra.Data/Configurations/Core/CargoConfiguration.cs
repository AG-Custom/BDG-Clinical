using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Core;

internal sealed class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
        builder.ToTable("cargo");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.Cargos)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}
