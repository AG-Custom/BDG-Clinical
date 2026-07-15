using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Packages;

internal sealed class CompraPacienteConfiguration : IEntityTypeConfiguration<CompraPaciente>
{
    public void Configure(EntityTypeBuilder<CompraPaciente> builder)
    {
        builder.ToTable("compra_paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente)
            .WithMany()
            .HasForeignKey(entity => entity.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Pacote)
            .WithMany(entity => entity.Compras)
            .HasForeignKey(entity => entity.PacoteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade)
            .WithMany()
            .HasForeignKey(entity => entity.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Status });
        builder.HasIndex(entity => entity.PacoteId);
        builder.HasIndex(entity => entity.UnidadeId);
    }
}
