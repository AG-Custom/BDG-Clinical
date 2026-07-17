using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Applications;

internal sealed class AplicacaoPacienteConfiguration : IEntityTypeConfiguration<AplicacaoPaciente>
{
    public void Configure(EntityTypeBuilder<AplicacaoPaciente> builder)
    {
        builder.ToTable("aplicacao_paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.QuantidadeUtilizada).HasPrecision(18, 4);
        builder.Property(entity => entity.Peso).HasPrecision(10, 3);
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany(entity => entity.Aplicacoes).HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cancelada });
        builder.HasOne(entity => entity.Produto).WithMany().HasForeignKey(entity => entity.ProdutoId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        builder.HasOne(entity => entity.Procedimento).WithMany().HasForeignKey(entity => entity.ProcedimentoId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        builder.HasOne(entity => entity.CompraPaciente)
            .WithMany(entity => entity.Aplicacoes)
            .HasForeignKey(entity => entity.CompraPacienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Agendamento)
            .WithMany(entity => entity.AplicacoesPaciente)
            .HasForeignKey(entity => entity.AgendamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataAplicacao });
    }
}
