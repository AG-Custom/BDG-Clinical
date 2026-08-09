using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AG.CLINICAL.Infra.Data.Configurations.Notifications;

internal sealed class OutputMessageEmailConfiguration : IEntityTypeConfiguration<OutputMessageEmail>
{
    public void Configure(EntityTypeBuilder<OutputMessageEmail> builder)
    {
        builder.ToTable("output_message_email");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(60).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.DestinatarioEmail).HasMaxLength(320).IsRequired();
        builder.Property(entity => entity.DestinatarioNome).HasMaxLength(200);
        builder.Property(entity => entity.PayloadJson).IsRequired();
        builder.Property(entity => entity.Erro).HasMaxLength(2000);
        builder.HasIndex(entity => new { entity.Status, entity.CriadoEm });
        builder.HasIndex(entity => entity.EmpresaId);
    }
}
