using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Modules;

internal sealed class PermissaoUsuarioConfiguration : IEntityTypeConfiguration<PermissaoUsuario>
{
    public void Configure(EntityTypeBuilder<PermissaoUsuario> builder)
    {
        builder.ToTable("permissao_usuario");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Usuario)
            .WithMany(entity => entity.Permissoes)
            .HasForeignKey(entity => entity.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Modulo)
            .WithMany(entity => entity.PermissoesUsuario)
            .HasForeignKey(entity => entity.ModuloId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.UsuarioId, entity.ModuloId }).IsUnique();
    }
}
