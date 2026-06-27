using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Modules;

internal sealed class LicencaModuloConfiguration : IEntityTypeConfiguration<LicencaModulo>
{
    public void Configure(EntityTypeBuilder<LicencaModulo> builder)
    {
        builder.ToTable("licenca_modulo");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Valor).HasPrecision(18, 2);
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.LicencasModulo)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Modulo)
            .WithMany(entity => entity.Licencas)
            .HasForeignKey(entity => entity.ModuloId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.ModuloId }).IsUnique();
    }
}
