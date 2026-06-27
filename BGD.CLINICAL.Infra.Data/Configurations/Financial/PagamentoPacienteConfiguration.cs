using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Financial;

internal sealed class PagamentoPacienteConfiguration : IEntityTypeConfiguration<PagamentoPaciente>
{
    public void Configure(EntityTypeBuilder<PagamentoPaciente> builder)
    {
        builder.ToTable("pagamento_paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ValorPago).HasPrecision(18, 2);
        builder.Property(entity => entity.Observacao).HasMaxLength(1000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ContaReceber).WithMany(entity => entity.Pagamentos).HasForeignKey(entity => entity.ContaReceberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.FormaPagamento).WithMany().HasForeignKey(entity => entity.FormaPagamentoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataPagamento });
    }
}
