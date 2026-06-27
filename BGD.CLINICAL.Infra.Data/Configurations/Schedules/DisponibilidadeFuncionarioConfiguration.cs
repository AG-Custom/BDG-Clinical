using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Schedules;

internal sealed class DisponibilidadeFuncionarioConfiguration : IEntityTypeConfiguration<DisponibilidadeFuncionario>
{
    public void Configure(EntityTypeBuilder<DisponibilidadeFuncionario> builder)
    {
        builder.ToTable("disponibilidade_funcionario");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.DiaSemana).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.UnidadeId, entity.DiaSemana });
    }
}
