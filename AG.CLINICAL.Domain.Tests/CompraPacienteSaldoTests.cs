using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;
using Xunit;

namespace AG.CLINICAL.Domain.Tests;

public sealed class CompraPacienteSaldoTests
{
    [Fact]
    public void Duas_compras_do_mesmo_pacote_ficam_com_saldo_independente()
    {
        var produtoId = Guid.NewGuid();
        var pacote = CriarPacoteComItem(produtoId, 5, "un");

        var compraA = CriarCompra(pacote);
        var compraB = CriarCompra(pacote);

        compraA.UpdateItemSaldo(produtoId, 5, 5, 0);

        Assert.Equal(0, compraA.GetQuantidadeRestante(produtoId));
        Assert.Equal(5, compraA.GetQuantidadeUtilizada(produtoId));
        Assert.Equal(5, compraB.GetQuantidadeRestante(produtoId));
        Assert.Equal(0, compraB.GetQuantidadeUtilizada(produtoId));
    }

    [Fact]
    public void Ajuste_esgota_e_reabre_somente_a_compra_ajustada()
    {
        var produtoId = Guid.NewGuid();
        var pacote = CriarPacoteComItem(produtoId, 5, "un");
        var compra = CriarCompra(pacote);
        var outra = CriarCompra(pacote);

        compra.UpdateItemSaldo(produtoId, 5, 5, 0);
        compra.CompleteIfExhausted();

        Assert.Equal(StatusCompraPaciente.Concluido, compra.Status);
        Assert.Equal(StatusCompraPaciente.Ativo, outra.Status);

        compra.UpdateItemSaldo(produtoId, 5, 3, 0);
        compra.ReopenIfCompleted();

        Assert.Equal(StatusCompraPaciente.Ativo, compra.Status);
        Assert.Equal(2, compra.GetQuantidadeRestante(produtoId));
        Assert.Equal(5, outra.GetQuantidadeRestante(produtoId));
    }

    [Fact]
    public void Aplicacao_rejeita_produto_ausente_e_quantidade_acima_do_saldo()
    {
        var produtoId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
        var pacote = CriarPacoteComItem(produtoId, 5, "un");
        var compra = CriarCompra(pacote, pacienteId);

        var ausente = Assert.Throws<DomainException>(
            () => compra.EnsurePodeAplicar(pacienteId, Guid.NewGuid(), 1));
        Assert.Contains("não existe nos itens", ausente.Message, StringComparison.OrdinalIgnoreCase);

        var excesso = Assert.Throws<DomainException>(
            () => compra.EnsurePodeAplicar(pacienteId, produtoId, 6));
        Assert.Contains("insuficiente", excesso.Message, StringComparison.OrdinalIgnoreCase);

        compra.EnsurePodeAplicar(pacienteId, produtoId, 2);
    }

    private static Pacote CriarPacoteComItem(Guid produtoId, decimal quantidade, string unidade)
    {
        var pacote = Pacote.Create(Guid.NewGuid(), "Pacote teste", null, 100);
        pacote.AddItem(produtoId, quantidade, unidade);
        return pacote;
    }

    private static CompraPaciente CriarCompra(Pacote pacote, Guid? pacienteId = null)
    {
        var compra = CompraPaciente.Create(
            pacote.EmpresaId,
            pacienteId ?? Guid.NewGuid(),
            pacote.Id,
            Guid.NewGuid(),
            DateTime.UtcNow);
        compra.CopiarItensDoPacote(pacote.Itens);
        return compra;
    }
}
