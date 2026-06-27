using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.GoogleCalendar;

internal sealed class AgendamentoGoogleSyncConfiguration : IEntityTypeConfiguration<AgendamentoGoogleSync>
{
    public void Configure(EntityTypeBuilder<AgendamentoGoogleSync> builder)
    {
        builder.ToTable("agendamento_google_sync");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.GoogleEventId).HasMaxLength(300);
        builder.Property(entity => entity.StatusSync).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.ErroSync).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Agendamento)
            .WithOne(entity => entity.GoogleSync)
            .HasForeignKey<AgendamentoGoogleSync>(entity => entity.AgendamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AgendaGoogle)
            .WithMany(entity => entity.Sincronizacoes)
            .HasForeignKey(entity => entity.AgendaGoogleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => entity.AgendamentoId).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.StatusSync });
    }
}
