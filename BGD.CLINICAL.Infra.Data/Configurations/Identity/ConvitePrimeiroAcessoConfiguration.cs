using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Identity;

internal sealed class ConvitePrimeiroAcessoConfiguration : IEntityTypeConfiguration<ConvitePrimeiroAcesso>
{
    public void Configure(EntityTypeBuilder<ConvitePrimeiroAcesso> builder)
    {
        builder.ToTable("convite_primeiro_acesso");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasOne(entity => entity.Usuario)
            .WithMany()
            .HasForeignKey(entity => entity.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.TokenHash).IsUnique();
        builder.HasIndex(entity => new { entity.UsuarioId, entity.UtilizadoEm });
    }
}
