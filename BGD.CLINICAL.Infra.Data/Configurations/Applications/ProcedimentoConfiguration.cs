using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Applications;

internal sealed class ProcedimentoConfiguration : IEntityTypeConfiguration<Procedimento>
{
    public void Configure(EntityTypeBuilder<Procedimento> builder)
    {
        builder.ToTable("procedimento");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Observacoes).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ProdutoAplicado)
            .WithMany()
            .HasForeignKey(entity => entity.ProdutoAplicadoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}
