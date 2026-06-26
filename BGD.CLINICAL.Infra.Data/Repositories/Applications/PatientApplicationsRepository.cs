using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BGD.CLINICAL.Infra.Data.Repositories.Applications;

public sealed class PatientApplicationsRepository : IPatientApplicationsRepository
{
    private readonly AppDbContext _context;

    public PatientApplicationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<AplicacaoPaciente?> GetByIdAndEmpresaIdWithDetailsAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.AplicacoesPaciente
            .Include(aplicacao => aplicacao.Paciente)
            .Include(aplicacao => aplicacao.Produto)
            .Include(aplicacao => aplicacao.Procedimento)
            .Include(aplicacao => aplicacao.Funcionario)
            .Include(aplicacao => aplicacao.Unidade)
            .Include(aplicacao => aplicacao.Sintomas)
                .ThenInclude(aplicacaoSintoma => aplicacaoSintoma.Sintoma)
            .Include(aplicacao => aplicacao.MovimentacoesEstoque)
                .ThenInclude(movimentacao => movimentacao.Produto)
            .FirstOrDefaultAsync(
                aplicacao => aplicacao.Id == id && aplicacao.EmpresaId == empresaId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AplicacaoPaciente>> ListByEmpresaIdAsync(
        Guid empresaId,
        Guid? pacienteId,
        Guid? unidadeId,
        Guid? produtoId,
        Guid? procedimentoId,
        Guid? aplicadorId,
        bool? cancelada,
        DateTime? dataInicio,
        DateTime? dataFim,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AplicacoesPaciente
            .AsNoTracking()
            .Include(aplicacao => aplicacao.Paciente)
            .Include(aplicacao => aplicacao.Produto)
            .Include(aplicacao => aplicacao.Procedimento)
            .Include(aplicacao => aplicacao.Funcionario)
            .Include(aplicacao => aplicacao.Unidade)
            .Include(aplicacao => aplicacao.Sintomas)
                .ThenInclude(aplicacaoSintoma => aplicacaoSintoma.Sintoma)
            .Where(aplicacao => aplicacao.EmpresaId == empresaId);

        if (pacienteId.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.PacienteId == pacienteId.Value);
        }

        if (unidadeId.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.UnidadeId == unidadeId.Value);
        }

        if (produtoId.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.ProdutoId == produtoId.Value);
        }

        if (procedimentoId.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.ProcedimentoId == procedimentoId.Value);
        }

        if (aplicadorId.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.FuncionarioId == aplicadorId.Value);
        }

        if (cancelada.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.Cancelada == cancelada.Value);
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.DataAplicacao >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(aplicacao => aplicacao.DataAplicacao <= dataFim.Value);
        }

        return await query
            .OrderByDescending(aplicacao => aplicacao.DataAplicacao)
            .ThenByDescending(aplicacao => aplicacao.CriadoEm)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsCompraPacienteForPacienteAsync(
        Guid compraPacienteId,
        Guid pacienteId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return _context.ComprasPaciente.AnyAsync(
            compra => compra.Id == compraPacienteId
                && compra.PacienteId == pacienteId
                && compra.EmpresaId == empresaId,
            cancellationToken);
    }

    public async Task AddAsync(AplicacaoPaciente aplicacao, CancellationToken cancellationToken = default)
    {
        await _context.AplicacoesPaciente.AddAsync(aplicacao, cancellationToken);
    }

    public void Update(AplicacaoPaciente aplicacao)
    {
        _context.AplicacoesPaciente.Update(aplicacao);
    }

    public async Task ReplaceSymptomsAsync(
        Guid aplicacaoPacienteId,
        IReadOnlyList<Guid> sintomaIds,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.AplicacoesSintomas
            .Where(aplicacaoSintoma => aplicacaoSintoma.AplicacaoPacienteId == aplicacaoPacienteId)
            .ToListAsync(cancellationToken);

        _context.AplicacoesSintomas.RemoveRange(existing);

        foreach (var sintomaId in sintomaIds.Distinct())
        {
            await _context.AplicacoesSintomas.AddAsync(
                new AplicacaoSintoma(aplicacaoPacienteId, sintomaId),
                cancellationToken);
        }
    }
}
