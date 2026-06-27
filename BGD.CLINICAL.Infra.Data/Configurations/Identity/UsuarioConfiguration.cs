using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Infra.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Identity;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.EmailLogin).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.SenhaHash).HasMaxLength(500);
        builder.Property(entity => entity.PendentePrimeiroAcesso).HasDefaultValue(false);
        builder.Property(entity => entity.AuthProvider).HasMaxLength(60).IsRequired();
        builder.Property(entity => entity.GoogleId).HasMaxLength(200);
        builder.Property(entity => entity.TipoUsuario)
            .HasConversion(new TipoUsuarioValueConverter())
            .HasMaxLength(40)
            .IsRequired();
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.Usuarios)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario)
            .WithMany()
            .HasForeignKey(entity => entity.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.EmailLogin }).IsUnique();
    }
}
