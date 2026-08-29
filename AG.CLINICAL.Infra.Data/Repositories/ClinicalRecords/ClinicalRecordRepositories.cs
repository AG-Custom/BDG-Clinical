using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.ClinicalRecords;

public sealed class MedicalRecordsRepository : IMedicalRecordsRepository
{
    private readonly AppDbContext _context;

    public MedicalRecordsRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Prontuario?> GetByPacienteIdAsync(Guid empresaId, Guid pacienteId, CancellationToken cancellationToken = default)
    {
        return _context.Prontuarios
            .Include(item => item.Paciente)
            .FirstOrDefaultAsync(
                item => item.EmpresaId == empresaId && item.PacienteId == pacienteId,
                cancellationToken);
    }

    public async Task AddAsync(Prontuario prontuario, CancellationToken cancellationToken = default)
    {
        await _context.Prontuarios.AddAsync(prontuario, cancellationToken);
    }

    public void Update(Prontuario prontuario)
    {
        if (_context.Entry(prontuario).State == EntityState.Detached)
        {
            _context.Prontuarios.Update(prontuario);
        }
    }

    public async Task AddEventoAsync(EventoClinico evento, CancellationToken cancellationToken = default)
    {
        await _context.EventosClinicos.AddAsync(evento, cancellationToken);
    }

    public async Task<IReadOnlyList<EventoClinico>> ListEventosByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await _context.EventosClinicos
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Where(item => item.EmpresaId == empresaId && item.PacienteId == pacienteId)
            .OrderByDescending(item => item.Data)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventoClinico>> ListEventosByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EventosClinicos
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId)
            .OrderByDescending(item => item.Data)
            .ToListAsync(cancellationToken);
    }
}

public sealed class ClinicalEncountersRepository : IClinicalEncountersRepository
{
    private readonly AppDbContext _context;

    public ClinicalEncountersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AtendimentoClinico>> ListByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AtendimentosClinicos
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Where(item => item.EmpresaId == empresaId && item.PacienteId == pacienteId);

        if (!includeInactive)
        {
            query = query.Where(item => item.Ativo);
        }

        return await query
            .OrderByDescending(item => item.DataInicio)
            .ToListAsync(cancellationToken);
    }

    public Task<AtendimentoClinico?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.AtendimentosClinicos
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Include(item => item.Paciente)
            .Include(item => item.Prontuario)
            .FirstOrDefaultAsync(
                item => item.Id == id && item.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<bool> HasOpenEncounterAsync(
        Guid empresaId,
        Guid pacienteId,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        return _context.AtendimentosClinicos.AnyAsync(
            item => item.EmpresaId == empresaId
                && item.PacienteId == pacienteId
                && item.Ativo
                && item.Status == StatusAtendimentoClinico.EmAndamento
                && (!excludeId.HasValue || item.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task AddAsync(AtendimentoClinico atendimento, CancellationToken cancellationToken = default)
    {
        await _context.AtendimentosClinicos.AddAsync(atendimento, cancellationToken);
    }

    public void Update(AtendimentoClinico atendimento)
    {
        if (_context.Entry(atendimento).State == EntityState.Detached)
        {
            _context.AtendimentosClinicos.Update(atendimento);
        }
    }
}

public sealed class ClinicalNotesRepository : IClinicalNotesRepository
{
    private readonly AppDbContext _context;

    public ClinicalNotesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AnotacaoClinica>> ListByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnotacoesClinicas
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId && item.Ativo)
            .OrderByDescending(item => item.Data)
            .ToListAsync(cancellationToken);
    }

    public Task<AnotacaoClinica?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.AnotacoesClinicas
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .FirstOrDefaultAsync(
                item => item.Id == id && item.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task AddAsync(AnotacaoClinica anotacao, CancellationToken cancellationToken = default)
    {
        await _context.AnotacoesClinicas.AddAsync(anotacao, cancellationToken);
    }

    public void Update(AnotacaoClinica anotacao)
    {
        if (_context.Entry(anotacao).State == EntityState.Detached)
        {
            _context.AnotacoesClinicas.Update(anotacao);
        }
    }
}

public sealed class AnamneseTemplatesRepository : IAnamneseTemplatesRepository
{
    private readonly AppDbContext _context;

    public AnamneseTemplatesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ModeloAnamnese>> ListByEmpresaIdAsync(
        Guid empresaId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ModelosAnamnese
            .AsNoTracking()
            .Where(item => item.EmpresaId == empresaId);

        if (!includeInactive)
        {
            query = query.Where(item => item.Ativo);
        }

        return await query.OrderBy(item => item.Nome).ToListAsync(cancellationToken);
    }

    public Task<ModeloAnamnese?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.ModelosAnamnese.FirstOrDefaultAsync(
            item => item.Id == id && item.EmpresaId == empresaId,
            cancellationToken);
    }

    public Task<bool> ExistsByNomeAsync(
        Guid empresaId,
        string nome,
        Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        var normalized = nome.Trim().ToUpperInvariant();
        return _context.ModelosAnamnese.AnyAsync(
            item => item.EmpresaId == empresaId
                && item.Nome.ToUpper() == normalized
                && (!excludeId.HasValue || item.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task AddAsync(ModeloAnamnese modelo, CancellationToken cancellationToken = default)
    {
        await _context.ModelosAnamnese.AddAsync(modelo, cancellationToken);
    }

    public void Update(ModeloAnamnese modelo)
    {
        if (_context.Entry(modelo).State == EntityState.Detached)
        {
            _context.ModelosAnamnese.Update(modelo);
        }
    }
}

public sealed class AnamneseRecordsRepository : IAnamneseRecordsRepository
{
    private readonly AppDbContext _context;

    public AnamneseRecordsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RegistroAnamnese>> ListByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RegistrosAnamnese
            .AsNoTracking()
            .Include(item => item.ModeloAnamnese)
            .Include(item => item.Funcionario)
            .Include(item => item.Versoes)
                .ThenInclude(item => item.Funcionario)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId && item.Ativo)
            .OrderByDescending(item => item.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public Task<RegistroAnamnese?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.RegistrosAnamnese
            .Include(item => item.ModeloAnamnese)
            .Include(item => item.Funcionario)
            .Include(item => item.Versoes)
                .ThenInclude(item => item.Funcionario)
            .FirstOrDefaultAsync(
                item => item.Id == id && item.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task AddAsync(RegistroAnamnese registro, CancellationToken cancellationToken = default)
    {
        await _context.RegistrosAnamnese.AddAsync(registro, cancellationToken);
    }

    public void Update(RegistroAnamnese registro)
    {
        var entry = _context.Entry(registro);
        if (entry.State == EntityState.Detached)
        {
            _context.RegistrosAnamnese.Update(registro);
        }

        foreach (var versao in registro.Versoes)
        {
            var versaoEntry = _context.Entry(versao);
            if (versaoEntry.State == EntityState.Detached)
            {
                _context.Set<RegistroAnamneseVersao>().Add(versao);
            }
        }
    }
}

public sealed class BodyAssessmentsRepository : IBodyAssessmentsRepository
{
    private readonly AppDbContext _context;

    public BodyAssessmentsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AvaliacaoCorporal>> ListByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AvaliacoesCorporais
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Where(item => item.EmpresaId == empresaId && item.PacienteId == pacienteId && item.Ativo)
            .OrderByDescending(item => item.DataAvaliacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvaliacaoCorporal>> ListByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AvaliacoesCorporais
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId && item.Ativo)
            .OrderByDescending(item => item.DataAvaliacao)
            .ToListAsync(cancellationToken);
    }

    public Task<AvaliacaoCorporal?> GetByIdAndEmpresaIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.AvaliacoesCorporais
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Include(item => item.Paciente)
            .FirstOrDefaultAsync(
                item => item.Id == id && item.EmpresaId == empresaId,
                cancellationToken);
    }

    public Task<AvaliacaoCorporal?> GetLatestByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        return _context.AvaliacoesCorporais
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Where(item => item.EmpresaId == empresaId && item.PacienteId == pacienteId && item.Ativo)
            .OrderByDescending(item => item.DataAvaliacao)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(AvaliacaoCorporal avaliacao, CancellationToken cancellationToken = default)
    {
        await _context.AvaliacoesCorporais.AddAsync(avaliacao, cancellationToken);
    }

    public void Update(AvaliacaoCorporal avaliacao)
    {
        if (_context.Entry(avaliacao).State == EntityState.Detached)
        {
            _context.AvaliacoesCorporais.Update(avaliacao);
        }
    }
}

public sealed class ClinicalFilesRepository : IClinicalFilesRepository
{
    private readonly AppDbContext _context;

    public ClinicalFilesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AnexoClinico>> ListAnexosByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        TipoAnexoClinico? tipo,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AnexosClinicos
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId && item.Ativo);

        if (tipo.HasValue)
        {
            query = query.Where(item => item.Tipo == tipo.Value);
        }

        return await query.OrderByDescending(item => item.DataDocumento).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AnexoClinico>> ListAnexosByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        TipoAnexoClinico? tipo,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AnexosClinicos
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Where(item => item.EmpresaId == empresaId && item.PacienteId == pacienteId && item.Ativo);

        if (tipo.HasValue)
        {
            query = query.Where(item => item.Tipo == tipo.Value);
        }

        return await query.OrderByDescending(item => item.DataDocumento).ToListAsync(cancellationToken);
    }

    public Task<AnexoClinico?> GetAnexoByIdAsync(Guid id, Guid empresaId, CancellationToken cancellationToken = default)
    {
        return _context.AnexosClinicos.FirstOrDefaultAsync(
            item => item.Id == id && item.EmpresaId == empresaId,
            cancellationToken);
    }

    public async Task AddAnexoAsync(AnexoClinico anexo, CancellationToken cancellationToken = default)
    {
        await _context.AnexosClinicos.AddAsync(anexo, cancellationToken);
    }

    public void UpdateAnexo(AnexoClinico anexo)
    {
        if (_context.Entry(anexo).State == EntityState.Detached)
        {
            _context.AnexosClinicos.Update(anexo);
        }
    }

    public async Task<IReadOnlyList<FotoComparativa>> ListFotosByPacienteAsync(
        Guid empresaId,
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.FotosComparativas
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Include(item => item.Unidade)
            .Where(item => item.EmpresaId == empresaId && item.PacienteId == pacienteId && item.Ativo)
            .OrderByDescending(item => item.DataCaptura)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FotoComparativa>> ListFotosByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.FotosComparativas
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId && item.Ativo)
            .OrderByDescending(item => item.DataCaptura)
            .ToListAsync(cancellationToken);
    }

    public Task<FotoComparativa?> GetFotoByIdAsync(Guid id, Guid empresaId, CancellationToken cancellationToken = default)
    {
        return _context.FotosComparativas
            .Include(item => item.Funcionario)
            .FirstOrDefaultAsync(
                item => item.Id == id && item.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task AddFotoAsync(FotoComparativa foto, CancellationToken cancellationToken = default)
    {
        await _context.FotosComparativas.AddAsync(foto, cancellationToken);
    }

    public void UpdateFoto(FotoComparativa foto)
    {
        if (_context.Entry(foto).State == EntityState.Detached)
        {
            _context.FotosComparativas.Update(foto);
        }
    }
}

public sealed class NutritionRecordsRepository : INutritionRecordsRepository
{
    private readonly AppDbContext _context;

    public NutritionRecordsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CalculoEnergeticoRegistro>> ListVentaByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CalculosEnergeticos
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId && item.Ativo)
            .OrderByDescending(item => item.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public Task<CalculoEnergeticoRegistro?> GetVentaByIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.CalculosEnergeticos.FirstOrDefaultAsync(
            item => item.Id == id && item.EmpresaId == empresaId,
            cancellationToken);
    }

    public async Task AddVentaAsync(CalculoEnergeticoRegistro registro, CancellationToken cancellationToken = default)
    {
        await _context.CalculosEnergeticos.AddAsync(registro, cancellationToken);
    }

    public async Task<IReadOnlyList<RegraBolsoRegistro>> ListRegraBolsoByAtendimentoAsync(
        Guid empresaId,
        Guid atendimentoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RegrasBolso
            .AsNoTracking()
            .Include(item => item.Funcionario)
            .Where(item => item.EmpresaId == empresaId && item.AtendimentoClinicoId == atendimentoId && item.Ativo)
            .OrderByDescending(item => item.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRegraBolsoAsync(RegraBolsoRegistro registro, CancellationToken cancellationToken = default)
    {
        await _context.RegrasBolso.AddAsync(registro, cancellationToken);
    }
}
