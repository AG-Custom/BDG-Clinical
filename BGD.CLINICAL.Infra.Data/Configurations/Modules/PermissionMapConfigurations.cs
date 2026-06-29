using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Modules;

internal sealed class UsuarioPermissaoOverrideConfiguration : IEntityTypeConfiguration<UsuarioPermissaoOverride>
{
    public void Configure(EntityTypeBuilder<UsuarioPermissaoOverride> builder)
    {
        builder.ToTable("usuario_permissao");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.PermissionKey).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Effect)
            .HasConversion(new PermissionEffectValueConverter())
            .HasMaxLength(10)
            .IsRequired();
        builder.HasOne(entity => entity.Usuario)
            .WithMany(entity => entity.PermissoesOverride)
            .HasForeignKey(entity => entity.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(entity => new { entity.UsuarioId, entity.PermissionKey }).IsUnique();
    }
}
