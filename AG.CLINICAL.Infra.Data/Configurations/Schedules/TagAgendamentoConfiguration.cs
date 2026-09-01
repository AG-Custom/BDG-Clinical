using AG.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AG.CLINICAL.Infra.Data.Configurations.Schedules;

internal sealed class TagAgendamentoConfiguration : IEntityTypeConfiguration<TagAgendamento>
{
    public void Configure(EntityTypeBuilder<TagAgendamento> builder)
    {
        builder.ToTable("tag_agendamento");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Cor).HasMaxLength(20).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}
