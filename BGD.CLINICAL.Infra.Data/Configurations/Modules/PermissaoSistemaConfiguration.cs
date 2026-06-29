using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Modules;

internal sealed class PermissaoSistemaConfiguration : IEntityTypeConfiguration<PermissaoSistema>
{
    public void Configure(EntityTypeBuilder<PermissaoSistema> builder)
    {
        builder.ToTable("permissao_sistema");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Chave).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Descricao).HasMaxLength(240).IsRequired();
        builder.Property(entity => entity.Categoria).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.ModuloCodigo).HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.ChavePai).HasMaxLength(120);
        builder.HasIndex(entity => entity.Chave).IsUnique();
    }
}
