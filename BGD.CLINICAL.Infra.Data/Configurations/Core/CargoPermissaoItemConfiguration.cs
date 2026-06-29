using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Core;

internal sealed class CargoPermissaoItemConfiguration : IEntityTypeConfiguration<CargoPermissaoItem>
{
    public void Configure(EntityTypeBuilder<CargoPermissaoItem> builder)
    {
        builder.ToTable("cargo_permissao_item");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.PermissionKey).HasMaxLength(120).IsRequired();
        builder.HasOne(entity => entity.Cargo)
            .WithMany(entity => entity.Permissoes)
            .HasForeignKey(entity => entity.CargoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(entity => new { entity.CargoId, entity.PermissionKey }).IsUnique();
    }
}
