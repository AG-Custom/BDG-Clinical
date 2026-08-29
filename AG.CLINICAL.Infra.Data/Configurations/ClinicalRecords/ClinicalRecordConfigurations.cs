using AG.CLINICAL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AG.CLINICAL.Infra.Data.Configurations.ClinicalRecords;

internal sealed class ProntuarioConfiguration : IEntityTypeConfiguration<Prontuario>
{
    public void Configure(EntityTypeBuilder<Prontuario> builder)
    {
        builder.ToTable("prontuario");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Alergias).HasMaxLength(4000);
        builder.Property(entity => entity.Alertas).HasMaxLength(4000);
        builder.Property(entity => entity.Observacao).HasMaxLength(4000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId }).IsUnique();
    }
}

internal sealed class AtendimentoClinicoConfiguration : IEntityTypeConfiguration<AtendimentoClinico>
{
    public void Configure(EntityTypeBuilder<AtendimentoClinico> builder)
    {
        builder.ToTable("atendimento_clinico");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(4000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Prontuario).WithMany(entity => entity.Atendimentos).HasForeignKey(entity => entity.ProntuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataInicio });
    }
}

internal sealed class EventoClinicoConfiguration : IEntityTypeConfiguration<EventoClinico>
{
    public void Configure(EntityTypeBuilder<EventoClinico> builder)
    {
        builder.ToTable("evento_clinico");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Resumo).HasMaxLength(1000);
        builder.Property(entity => entity.Entidade).HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.DadosAnteriores).HasMaxLength(4000);
        builder.Property(entity => entity.DadosNovos).HasMaxLength(4000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.Eventos).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.Data });
    }
}

internal sealed class AnotacaoClinicaConfiguration : IEntityTypeConfiguration<AnotacaoClinica>
{
    public void Configure(EntityTypeBuilder<AnotacaoClinica> builder)
    {
        builder.ToTable("anotacao_clinica");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Texto).HasMaxLength(8000).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.Anotacoes).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ModeloAnamneseConfiguration : IEntityTypeConfiguration<ModeloAnamnese>
{
    public void Configure(EntityTypeBuilder<ModeloAnamnese> builder)
    {
        builder.ToTable("modelo_anamnese");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Especialidade).HasMaxLength(120);
        builder.Property(entity => entity.SchemaJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.Nome }).IsUnique();
    }
}

internal sealed class RegistroAnamneseConfiguration : IEntityTypeConfiguration<RegistroAnamnese>
{
    public void Configure(EntityTypeBuilder<RegistroAnamnese> builder)
    {
        builder.ToTable("registro_anamnese");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.SchemaSnapshotJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(entity => entity.RespostasJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.Anamneses).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.ModeloAnamnese).WithMany().HasForeignKey(entity => entity.ModeloAnamneseId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RegistroAnamneseVersaoConfiguration : IEntityTypeConfiguration<RegistroAnamneseVersao>
{
    public void Configure(EntityTypeBuilder<RegistroAnamneseVersao> builder)
    {
        builder.ToTable("registro_anamnese_versao");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RespostasJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(entity => entity.RespostasAnterioresJson).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.ResumoAlteracao).HasMaxLength(1000);
        builder.HasOne(entity => entity.RegistroAnamnese).WithMany(entity => entity.Versoes).HasForeignKey(entity => entity.RegistroAnamneseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AvaliacaoCorporalConfiguration : IEntityTypeConfiguration<AvaliacaoCorporal>
{
    public void Configure(EntityTypeBuilder<AvaliacaoCorporal> builder)
    {
        builder.ToTable("avaliacao_corporal");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.AlturaCm).HasPrecision(10, 2);
        builder.Property(entity => entity.PesoKg).HasPrecision(10, 3);
        builder.Property(entity => entity.Imc).HasPrecision(10, 2);
        builder.Property(entity => entity.ClassificacaoImc).HasMaxLength(40);
        builder.Property(entity => entity.PesoIdealKg).HasPrecision(10, 3);
        builder.Property(entity => entity.CinturaCm).HasPrecision(10, 2);
        builder.Property(entity => entity.QuadrilCm).HasPrecision(10, 2);
        builder.Property(entity => entity.RelacaoCinturaQuadril).HasPrecision(10, 3);
        builder.Property(entity => entity.PercentualMassaGorda).HasPrecision(10, 2);
        builder.Property(entity => entity.MassaGordaKg).HasPrecision(10, 3);
        builder.Property(entity => entity.PercentualMassaMagra).HasPrecision(10, 2);
        builder.Property(entity => entity.MassaMagraKg).HasPrecision(10, 3);
        builder.Property(entity => entity.PercentualAgua).HasPrecision(10, 2);
        builder.Property(entity => entity.ProtocoloPrega).HasConversion<string>().HasMaxLength(40);
        builder.Property(entity => entity.BioimpedanciaJson).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.CircunferenciasJson).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.PregasJson).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.Observacao).HasMaxLength(4000);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.Avaliacoes).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(entity => new { entity.EmpresaId, entity.PacienteId, entity.DataAvaliacao });
    }
}

internal sealed class AnexoClinicoConfiguration : IEntityTypeConfiguration<AnexoClinico>
{
    public void Configure(EntityTypeBuilder<AnexoClinico> builder)
    {
        builder.ToTable("anexo_clinico");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Nome).HasMaxLength(180).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.Property(entity => entity.CategoriaExame).HasMaxLength(40);
        builder.Property(entity => entity.CategoriaDocumento).HasMaxLength(40);
        builder.Property(entity => entity.NomeArquivo).HasMaxLength(260).IsRequired();
        builder.Property(entity => entity.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.ObjectKey).HasMaxLength(500).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.Anexos).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class FotoComparativaConfiguration : IEntityTypeConfiguration<FotoComparativa>
{
    public void Configure(EntityTypeBuilder<FotoComparativa> builder)
    {
        builder.ToTable("foto_comparativa");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Categoria).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Observacao).HasMaxLength(2000);
        builder.Property(entity => entity.NomeArquivo).HasMaxLength(260).IsRequired();
        builder.Property(entity => entity.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.ObjectKey).HasMaxLength(500).IsRequired();
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.Fotos).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AvaliacaoCorporal).WithMany().HasForeignKey(entity => entity.AvaliacaoCorporalId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CalculoEnergeticoRegistroConfiguration : IEntityTypeConfiguration<CalculoEnergeticoRegistro>
{
    public void Configure(EntityTypeBuilder<CalculoEnergeticoRegistro> builder)
    {
        builder.ToTable("calculo_energetico");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Perfil).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.Protocolo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.NivelAtividade).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.FatorInjuria).HasPrecision(8, 3);
        builder.Property(entity => entity.PesoKg).HasPrecision(10, 3);
        builder.Property(entity => entity.AlturaCm).HasPrecision(10, 2);
        builder.Property(entity => entity.MassaMagraKg).HasPrecision(10, 3);
        builder.Property(entity => entity.PesoDesejadoKg).HasPrecision(10, 3);
        builder.Property(entity => entity.AtividadesJson).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.GastoEnergeticoBasal).HasPrecision(12, 2);
        builder.Property(entity => entity.GastoEnergeticoTotal).HasPrecision(12, 2);
        builder.Property(entity => entity.AjusteCaloricoDiario).HasPrecision(12, 2);
        builder.Property(entity => entity.MetaCaloricaDiaria).HasPrecision(12, 2);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.CalculosEnergeticos).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RegraBolsoRegistroConfiguration : IEntityTypeConfiguration<RegraBolsoRegistro>
{
    public void Configure(EntityTypeBuilder<RegraBolsoRegistro> builder)
    {
        builder.ToTable("regra_bolso");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Objetivo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.PesoKg).HasPrecision(10, 3);
        builder.Property(entity => entity.GastoEnergeticoTotal).HasPrecision(12, 2);
        builder.Property(entity => entity.Calorias).HasPrecision(12, 2);
        builder.Property(entity => entity.ProteinasG).HasPrecision(10, 2);
        builder.Property(entity => entity.CarboidratosG).HasPrecision(10, 2);
        builder.Property(entity => entity.GordurasG).HasPrecision(10, 2);
        builder.HasOne(entity => entity.Empresa).WithMany().HasForeignKey(entity => entity.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.AtendimentoClinico).WithMany(entity => entity.RegrasBolso).HasForeignKey(entity => entity.AtendimentoClinicoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Paciente).WithMany().HasForeignKey(entity => entity.PacienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Unidade).WithMany().HasForeignKey(entity => entity.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Funcionario).WithMany().HasForeignKey(entity => entity.FuncionarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
