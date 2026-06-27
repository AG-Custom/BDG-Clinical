using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Modules;

internal sealed class ModuloSistemaConfiguration : IEntityTypeConfiguration<ModuloSistema>
{
    public void Configure(EntityTypeBuilder<ModuloSistema> builder)
    {
        builder.ToTable("modulo_sistema");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Codigo).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Descricao).HasMaxLength(500);
        builder.HasIndex(entity => entity.Codigo).IsUnique();
    }
}
