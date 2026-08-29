using AG.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AG.CLINICAL.Infra.Data.Configurations.Packages;

internal sealed class ItemCompraPacienteConfiguration : IEntityTypeConfiguration<ItemCompraPaciente>
{
    public void Configure(EntityTypeBuilder<ItemCompraPaciente> builder)
    {
        builder.ToTable("item_compra_paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.QuantidadeContratada).HasPrecision(18, 4);
        builder.Property(entity => entity.QuantidadeUtilizadaBase).HasPrecision(18, 4);
        builder.Property(entity => entity.UnidadeMedida).HasMaxLength(30).IsRequired();
        builder.HasOne(entity => entity.CompraPaciente)
            .WithMany(entity => entity.Itens)
            .HasForeignKey(entity => entity.CompraPacienteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Produto)
            .WithMany()
            .HasForeignKey(entity => entity.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.CompraPacienteId, entity.ProdutoId }).IsUnique();
    }
}
