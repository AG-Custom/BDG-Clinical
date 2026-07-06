using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Schedules;

internal sealed class AgendamentoProcedimentoConfiguration : IEntityTypeConfiguration<AgendamentoProcedimento>
{
    public void Configure(EntityTypeBuilder<AgendamentoProcedimento> builder)
    {
        builder.ToTable("agendamento_procedimento");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Agendamento)
            .WithMany(entity => entity.ProcedimentosVinculados)
            .HasForeignKey(entity => entity.AgendamentoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Procedimento)
            .WithMany()
            .HasForeignKey(entity => entity.ProcedimentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.AgendamentoId, entity.ProcedimentoId }).IsUnique();
    }
}
