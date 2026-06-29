using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Inventory;

internal sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedor");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.Property(entity => entity.Cnpj).HasMaxLength(20).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cnpj }).IsUnique();
    }
}
