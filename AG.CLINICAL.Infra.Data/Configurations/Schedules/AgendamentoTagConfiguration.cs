using AG.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AG.CLINICAL.Infra.Data.Configurations.Schedules;

internal sealed class AgendamentoTagConfiguration : IEntityTypeConfiguration<AgendamentoTag>
{
    public void Configure(EntityTypeBuilder<AgendamentoTag> builder)
    {
        builder.ToTable("agendamento_tag");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Ordem).IsRequired();
        builder.HasOne(entity => entity.Agendamento)
            .WithMany(entity => entity.TagsVinculadas)
            .HasForeignKey(entity => entity.AgendamentoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Tag)
            .WithMany(entity => entity.Agendamentos)
            .HasForeignKey(entity => entity.TagAgendamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.AgendamentoId, entity.TagAgendamentoId }).IsUnique();
        builder.HasIndex(entity => new { entity.AgendamentoId, entity.Ordem });
    }
}
