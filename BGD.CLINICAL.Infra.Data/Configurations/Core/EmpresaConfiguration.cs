using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Core;

internal sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresa");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Cnpj).HasMaxLength(20);
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.Property(entity => entity.Logo).HasMaxLength(500);
        builder.Property(entity => entity.CorPrincipal).HasMaxLength(20);
        builder.HasIndex(entity => entity.Cnpj);
    }
}
