using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Financial;

internal sealed class ContaReceberConfiguration : IEntityTypeConfiguration<ContaReceber>
{
    public void Configure(EntityTypeBuilder<ContaReceber> builder)
    {
        builder.ToTable("conta_receber");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ValorTotal).HasPrecision(18, 2);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CompraPaciente).WithMany().HasForeignKey(entity => entity.CompraPacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Status });
    }
}
