using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Schedules;

internal sealed class BloqueioAgendaConfiguration : IEntityTypeConfiguration<BloqueioAgenda>
{
    public void Configure(EntityTypeBuilder<BloqueioAgenda> builder)
    {
        builder.ToTable("bloqueio_agenda");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Motivo).HasMaxLength(500).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.CriadoPor).WithMany().HasForeignKey(entity => entity.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.DataInicio, entity.DataFim });
    }
}
