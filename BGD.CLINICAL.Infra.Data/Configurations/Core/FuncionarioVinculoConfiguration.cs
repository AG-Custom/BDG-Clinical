using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Core;

internal sealed class FuncionarioVinculoConfiguration : IEntityTypeConfiguration<FuncionarioVinculo>
{
    public void Configure(EntityTypeBuilder<FuncionarioVinculo> builder)
    {
        builder.ToTable("funcionario_vinculo");
        builder.HasKey(entity => entity.Id);
        builder.HasOne(entity => entity.Empresa)
            .WithMany(entity => entity.FuncionarioVinculos)
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade)
            .WithMany()
            .HasForeignKey(entity => entity.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Cargo)
            .WithMany(entity => entity.FuncionarioVinculos)
            .HasForeignKey(entity => entity.CargoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.FuncionarioId, entity.EmpresaId })
            .IsUnique()
            .HasFilter("[empresa_id] IS NOT NULL");
        builder.HasIndex(entity => new { entity.FuncionarioId, entity.UnidadeId })
            .IsUnique()
            .HasFilter("[unidade_id] IS NOT NULL");
        builder.ToTable(table => table.HasCheckConstraint(
            "ck_funcionario_vinculo_empresa_xor_unidade",
            "([empresa_id] IS NOT NULL AND [unidade_id] IS NULL) OR ([empresa_id] IS NULL AND [unidade_id] IS NOT NULL)"));
    }
}
