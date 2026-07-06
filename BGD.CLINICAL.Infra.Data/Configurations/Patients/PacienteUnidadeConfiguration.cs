using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Patients;

internal sealed class PacienteUnidadeConfiguration : IEntityTypeConfiguration<PacienteUnidade>
{
    public void Configure(EntityTypeBuilder<PacienteUnidade> builder)
    {
        builder.ToTable("paciente_unidade");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Paciente)
            .WithMany(entity => entity.UnidadesVinculadas)
            .HasForeignKey(entity => entity.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Unidade)
            .WithMany()
            .HasForeignKey(entity => entity.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.PacienteId, entity.UnidadeId }).IsUnique();
    }
}
