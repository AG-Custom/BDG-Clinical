using BGD.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BGD.CLINICAL.Infra.Data.Configurations.Patients;

internal sealed class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("paciente");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Cpf).HasMaxLength(20);
        builder.Property(entity => entity.Telefone).HasMaxLength(30);
        builder.Property(entity => entity.Email).HasMaxLength(200);
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);

        builder.OwnsOne(entity => entity.Endereco, endereco =>
        {
            endereco.WithOwner().HasForeignKey("PacienteId");
            endereco.Property<Guid>("PacienteId").HasColumnName("id");
            endereco.Property(value => value.Cep).HasColumnName("cep").HasMaxLength(8);
            endereco.Property(value => value.Logradouro).HasColumnName("logradouro").HasMaxLength(200);
            endereco.Property(value => value.Numero).HasColumnName("numero").HasMaxLength(20);
            endereco.Property(value => value.Complemento).HasColumnName("complemento").HasMaxLength(120);
            endereco.Property(value => value.Bairro).HasColumnName("bairro").HasMaxLength(120);
            endereco.Property(value => value.Cidade).HasColumnName("cidade").HasMaxLength(120);
            endereco.Property(value => value.Uf).HasColumnName("uf").HasMaxLength(2);
        });

        builder.Navigation(entity => entity.Endereco).IsRequired(false);

        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Cpf })
            .IsUnique()
            .HasFilter("[cpf] IS NOT NULL");
    }
}
