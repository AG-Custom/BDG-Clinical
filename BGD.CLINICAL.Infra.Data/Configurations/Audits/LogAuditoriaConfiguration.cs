using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Audits;

internal sealed class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("log_auditoria");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Entidade).HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Acao).HasConversion<string>().HasMaxLength(60).IsRequired();
        builder.Property(entity => entity.DadosAnteriores).HasColumnType("jsonb");
        builder.Property(entity => entity.DadosNovos).HasColumnType("jsonb");
        builder.Property(entity => entity.Ip).HasMaxLength(80);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Usuario).WithMany().HasForeignKey(entity => entity.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Entidade, entity.RegistroId });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.UsuarioId, entity.Data });
    }
}
