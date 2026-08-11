using AG.CLINICAL.Application.Inventory;
using AG.CLINICAL.Application.Reports.Abstractions;
using AG.CLINICAL.Application.Reports.Dtos;
using AG.CLINICAL.Domain.Constants;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AG.CLINICAL.Infra.Data.Repositories.Reports;

public sealed class OperationalReportsRepository(AppDbContext context) : IOperationalReportsRepository
{
    public async Task<OperationalReportDto> GetAsync(Guid empresaId, DateOnly dataInicio, DateOnly dataFim, CancellationToken cancellationToken = default)
    {
        var inicio = DateTime.SpecifyKind(dataInicio.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var fimExclusivo = DateTime.SpecifyKind(dataFim.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        // Joins explícitos evitam que o EF gere LEFT JOIN para a navegação opcional
        // AplicacaoPaciente e falhe ao traduzir o agrupamento completo.
        var medicamentosAgrupados = await (
                from movimento in context.MovimentacoesEstoque.AsNoTracking()
                join aplicacaoRegistrada in context.AplicacoesPaciente.AsNoTracking()
                    on movimento.AplicacaoPacienteId equals (Guid?)aplicacaoRegistrada.Id
                join produto in context.Produtos.AsNoTracking()
                    on movimento.ProdutoId equals produto.Id
                join tipoProduto in context.TiposProduto.AsNoTracking()
                    on produto.TipoProdutoId equals tipoProduto.Id
                join unidadeMedida in context.UnidadesMedida.AsNoTracking()
                    on produto.UnidadeMedidaId equals unidadeMedida.Id
                where movimento.EmpresaId == empresaId
                    && aplicacaoRegistrada.EmpresaId == empresaId
                    && movimento.Data >= inicio
                    && movimento.Data < fimExclusivo
                    && movimento.Tipo == TipoMovimentacaoEstoque.Saida
                    && movimento.Motivo == MotivoMovimentacaoEstoque.Aplicacao
                    && !aplicacaoRegistrada.Cancelada
                    && tipoProduto.Codigo == ProductTypeCodes.Medicamento
                group movimento by new
                {
                    movimento.ProdutoId,
                    produto.Nome,
                    UnidadeMedida = unidadeMedida.Sigla
                }
                into grupo
                select new
                {
                    grupo.Key.ProdutoId,
                    ProdutoNome = grupo.Key.Nome,
                    grupo.Key.UnidadeMedida,
                    Quantidade = grupo.Sum(movimento => movimento.Quantidade)
                })
            .ToListAsync(cancellationToken);

        var medicamentos = medicamentosAgrupados
            .OrderByDescending(item => item.Quantidade)
            .ThenBy(item => item.ProdutoNome)
            .Select(item => new MedicationUsageReportItemDto(
                item.ProdutoId,
                item.ProdutoNome,
                item.UnidadeMedida,
                item.Quantidade))
            .ToList();

        var porTipo = await context.Agendamentos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.DataInicio >= inicio && a.DataInicio < fimExclusivo
                && a.Status != StatusAgendamento.Cancelado
                && (a.Tipo == TipoAgendamento.Consulta || a.Tipo == TipoAgendamento.Aplicacao || a.Tipo == TipoAgendamento.Retorno))
            .GroupBy(a => a.Tipo).Select(g => new { Tipo = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Tipo, x => x.Quantidade, cancellationToken);

        var movimentos = await context.MovimentacoesEstoque.AsNoTracking()
            .Where(m =>
                m.EmpresaId == empresaId
                && m.Data >= inicio
                && m.Data < fimExclusivo
                && m.Motivo != MotivoMovimentacaoEstoque.Transferencia
                && m.Origem != "RESET_ESTOQUE")
            .Select(m => new
            {
                m.Tipo,
                m.Quantidade,
                m.ValorUnitario,
                ValorPedido = m.PedidoFornecedorId == null
                    ? null
                    : context.ItensPedidoFornecedor
                        .Where(item => item.PedidoFornecedorId == m.PedidoFornecedorId && item.ProdutoId == m.ProdutoId)
                        .Select(item => (decimal?)item.ValorUnitario)
                        .FirstOrDefault(),
                ValorProduto = m.Produto.Valor,
                m.Produto.ConteudoPorEmbalagem,
                m.Produto.ConcentracaoPorConteudo
            })
            .ToListAsync(cancellationToken);

        decimal entradas = 0, saidas = 0;
        foreach (var movimento in movimentos)
        {
            var fator = ProductStockValuation.ResolveFatorEmbalagemParaEstoque(movimento.ConteudoPorEmbalagem, movimento.ConcentracaoPorConteudo);
            var valorUnitario = movimento.ValorUnitario
                ?? ProductStockValuation.ResolveValorPorUnidadeEstoque(
                    movimento.ValorPedido ?? movimento.ValorProduto,
                    fator);
            var total = movimento.Quantidade * valorUnitario;
            if (movimento.Tipo is TipoMovimentacaoEstoque.Entrada or TipoMovimentacaoEstoque.Ajuste) entradas += total;
            else if (movimento.Tipo is TipoMovimentacaoEstoque.Saida or TipoMovimentacaoEstoque.Perda) saidas += total;
        }

        entradas = decimal.Round(entradas, 2, MidpointRounding.AwayFromZero);
        saidas = decimal.Round(saidas, 2, MidpointRounding.AwayFromZero);
        var consulta = porTipo.GetValueOrDefault(TipoAgendamento.Consulta);
        var aplicacao = porTipo.GetValueOrDefault(TipoAgendamento.Aplicacao);
        var retorno = porTipo.GetValueOrDefault(TipoAgendamento.Retorno);

        return new(dataInicio, dataFim, medicamentos,
            new(consulta, aplicacao, retorno, consulta + aplicacao + retorno),
            new(entradas, saidas, entradas - saidas));
    }
}
