using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Domain.Services;

public static class CalculoEnergetico
{
    public static decimal ObterFatorAtividade(NivelAtividadeFisica nivel)
    {
        return nivel switch
        {
            NivelAtividadeFisica.Sedentario => 1.2m,
            NivelAtividadeFisica.Leve => 1.375m,
            NivelAtividadeFisica.Moderado => 1.55m,
            NivelAtividadeFisica.Intenso => 1.725m,
            NivelAtividadeFisica.MuitoIntenso => 1.9m,
            _ => 1.2m
        };
    }

    public static decimal? CalcularTaxaMetabolicaBasal(
        ProtocoloGastoEnergetico protocolo,
        Sexo sexo,
        decimal pesoKg,
        decimal alturaCm,
        int idade,
        decimal? massaMagraKg)
    {
        if (pesoKg <= 0 || alturaCm <= 0 || idade <= 0)
        {
            return null;
        }

        decimal valor = protocolo switch
        {
            ProtocoloGastoEnergetico.HarrisBenedict => CalcularHarrisBenedict(sexo, pesoKg, alturaCm, idade),
            ProtocoloGastoEnergetico.MifflinStJeor => CalcularMifflin(sexo, pesoKg, alturaCm, idade),
            ProtocoloGastoEnergetico.KatchMcArdle => massaMagraKg is null or <= 0
                ? 0
                : 370m + (21.6m * massaMagraKg.Value),
            ProtocoloGastoEnergetico.Cunningham => massaMagraKg is null or <= 0
                ? 0
                : 500m + (22m * massaMagraKg.Value),
            _ => 0
        };

        return valor <= 0 ? null : decimal.Round(valor, 0, MidpointRounding.AwayFromZero);
    }

    public static decimal CalcularGastoAtividades(
        decimal pesoKg,
        IReadOnlyList<AtividadeFisicaDetalhe> atividades)
    {
        if (pesoKg <= 0 || atividades.Count == 0)
        {
            return 0;
        }

        var total = 0m;
        foreach (var atividade in atividades)
        {
            if (atividade.Mets <= 0 || atividade.Minutos <= 0)
            {
                continue;
            }

            total += atividade.Mets * pesoKg * (atividade.Minutos / 60m);
        }

        return decimal.Round(total, 0, MidpointRounding.AwayFromZero);
    }

    public static ResultadoVenta CalcularVenta(
        decimal taxaMetabolicaBasal,
        NivelAtividadeFisica nivelAtividade,
        decimal? fatorInjuria,
        decimal gastoAtividades,
        decimal pesoAtualKg,
        decimal pesoDesejadoKg,
        int tempoDias)
    {
        var fatorAtividade = ObterFatorAtividade(nivelAtividade);
        var injuria = fatorInjuria is null or <= 0 ? 1m : fatorInjuria.Value;
        var gastoTotal = decimal.Round(
            (taxaMetabolicaBasal * fatorAtividade * injuria) + gastoAtividades,
            0,
            MidpointRounding.AwayFromZero);

        var dias = tempoDias <= 0 ? 1 : tempoDias;
        var diferencaKg = pesoDesejadoKg - pesoAtualKg;
        var ajusteDiario = decimal.Round(diferencaKg * 7700m / dias, 0, MidpointRounding.AwayFromZero);
        var metaCalorica = decimal.Round(gastoTotal + ajusteDiario, 0, MidpointRounding.AwayFromZero);
        if (metaCalorica < 800)
        {
            metaCalorica = 800;
        }

        return new ResultadoVenta(
            taxaMetabolicaBasal,
            gastoTotal,
            ajusteDiario,
            metaCalorica,
            fatorAtividade);
    }

    public static ResultadoRegraBolso CalcularRegraBolso(
        ObjetivoRegraBolso objetivo,
        decimal pesoKg,
        decimal gastoEnergeticoTotal)
    {
        var calorias = objetivo switch
        {
            ObjetivoRegraBolso.Emagrecimento => gastoEnergeticoTotal * 0.80m,
            ObjetivoRegraBolso.Hipertrofia => gastoEnergeticoTotal * 1.10m,
            _ => gastoEnergeticoTotal
        };

        calorias = decimal.Round(calorias, 0, MidpointRounding.AwayFromZero);
        if (calorias < 800)
        {
            calorias = 800;
        }

        var proteinaGKg = objetivo switch
        {
            ObjetivoRegraBolso.Emagrecimento => 2.0m,
            ObjetivoRegraBolso.Hipertrofia => 2.2m,
            _ => 1.6m
        };

        var gorduraGKg = objetivo == ObjetivoRegraBolso.Emagrecimento ? 0.8m : 1.0m;
        var proteinasG = decimal.Round(pesoKg * proteinaGKg, 0, MidpointRounding.AwayFromZero);
        var gordurasG = decimal.Round(pesoKg * gorduraGKg, 0, MidpointRounding.AwayFromZero);
        var kcalProteinas = proteinasG * 4m;
        var kcalGorduras = gordurasG * 9m;
        var kcalCarboidratos = calorias - kcalProteinas - kcalGorduras;
        if (kcalCarboidratos < 0)
        {
            kcalCarboidratos = 0;
        }

        var carboidratosG = decimal.Round(kcalCarboidratos / 4m, 0, MidpointRounding.AwayFromZero);

        return new ResultadoRegraBolso(calorias, proteinasG, carboidratosG, gordurasG);
    }

    private static decimal CalcularHarrisBenedict(Sexo sexo, decimal pesoKg, decimal alturaCm, int idade)
    {
        if (sexo == Sexo.Masculino)
        {
            return 66.473m + (13.7516m * pesoKg) + (5.0033m * alturaCm) - (6.755m * idade);
        }

        return 655.0955m + (9.5634m * pesoKg) + (1.8496m * alturaCm) - (4.6756m * idade);
    }

    private static decimal CalcularMifflin(Sexo sexo, decimal pesoKg, decimal alturaCm, int idade)
    {
        var baseCalculo = (10m * pesoKg) + (6.25m * alturaCm) - (5m * idade);
        return sexo == Sexo.Masculino ? baseCalculo + 5m : baseCalculo - 161m;
    }
}

public sealed record AtividadeFisicaDetalhe(string Nome, decimal Mets, decimal Minutos);

public sealed record ResultadoVenta(
    decimal GastoEnergeticoBasal,
    decimal GastoEnergeticoTotal,
    decimal AjusteCaloricoDiario,
    decimal MetaCaloricaDiaria,
    decimal FatorAtividade);

public sealed record ResultadoRegraBolso(
    decimal Calorias,
    decimal ProteinasG,
    decimal CarboidratosG,
    decimal GordurasG);
