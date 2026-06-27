using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Schedules;

internal sealed class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.ToTable("agendamento");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.Property(entity => entity.MotivoCancelamento).HasMaxLength(500);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CompraPaciente).WithMany(entity => entity.Agendamentos).HasForeignKey(entity => entity.CompraPacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CriadoPor).WithMany().HasForeignKey(entity => entity.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CanceladoPor).WithMany().HasForeignKey(entity => entity.CanceladoPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.UnidadeId, entity.DataInicio });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.DataInicio, entity.DataFim });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Status });
    }
}
