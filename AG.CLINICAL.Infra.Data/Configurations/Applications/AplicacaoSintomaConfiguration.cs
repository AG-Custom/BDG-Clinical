using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AG.CLINICAL.Infra.Data.Configurations.Applications;

internal sealed class AplicacaoSintomaConfiguration : IEntityTypeConfiguration<AplicacaoSintoma>
{
    public void Configure(EntityTypeBuilder<AplicacaoSintoma> builder)
    {
        builder.ToTable("aplicacao_sintoma");
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => new { entity.AplicacaoPacienteId, entity.SintomaId }).IsUnique();
        builder.HasOne(entity => entity.AplicacaoPaciente)
            .WithMany(entity => entity.Sintomas)
            .HasForeignKey(entity => entity.AplicacaoPacienteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Sintoma)
            .WithMany(entity => entity.Aplicacoes)
            .HasForeignKey(entity => entity.SintomaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
