using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Packages;

internal sealed class PacoteConfiguration : IEntityTypeConfiguration<Pacote>
{
    public void Configure(EntityTypeBuilder<Pacote> builder)
    {
        builder.ToTable("pacote");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Descricao).HasMaxLength(1000);
        builder.Property(entity => entity.Valor).HasPrecision(18, 2);
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}
