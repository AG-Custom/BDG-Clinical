using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Core;

internal sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionario");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.HasMany(entity => entity.Vinculos)
            .WithOne(entity => entity.Funcionario)
            .HasForeignKey(entity => entity.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
