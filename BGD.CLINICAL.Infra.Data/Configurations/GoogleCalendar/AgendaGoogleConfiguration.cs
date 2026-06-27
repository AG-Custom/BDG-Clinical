using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.GoogleCalendar;

internal sealed class AgendaGoogleConfiguration : IEntityTypeConfiguration<AgendaGoogle>
{
    public void Configure(EntityTypeBuilder<AgendaGoogle> builder)
    {
        builder.ToTable("agenda_google");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.GoogleCalendarId).HasMaxLength(300).IsRequired();
        builder.Property(entity => entity.Nome).HasMaxLength(200).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ContaGoogleConectada)
            .WithMany(entity => entity.Agendas)
            .HasForeignKey(entity => entity.ContaGoogleConectadaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.ContaGoogleConectadaId, entity.GoogleCalendarId }).IsUnique();
    }
}
