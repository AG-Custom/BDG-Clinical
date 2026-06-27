using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.GoogleCalendar;

internal sealed class ContaGoogleConectadaConfiguration : IEntityTypeConfiguration<ContaGoogleConectada>
{
    public void Configure(EntityTypeBuilder<ContaGoogleConectada> builder)
    {
        builder.ToTable("conta_google_conectada");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.GoogleEmail).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.GoogleAccountId).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.AccessToken).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.RefreshToken).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.EscoposAutorizados).HasMaxLength(1000).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Usuario).WithMany().HasForeignKey(entity => entity.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.GoogleAccountId });
        builder.HasIndex(entity => new { entity.EmpresaId, entity.FuncionarioId, entity.Ativo });
    }
}
