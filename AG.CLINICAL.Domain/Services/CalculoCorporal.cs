using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Domain.Services;

public static class CalculoCorporal
{
    public static decimal? CalcularImc(decimal? pesoKg, decimal? alturaCm)
    {
        if (pesoKg is null or <= 0 || alturaCm is null or <= 0)
        {
            return null;
        }

        var alturaM = alturaCm.Value / 100m;
        return decimal.Round(pesoKg.Value / (alturaM * alturaM), 2, MidpointRounding.AwayFromZero);
    }

    public static string? ClassificarImc(decimal? imc)
    {
        if (imc is null)
        {
            return null;
        }

        return imc.Value switch
        {
            < 18.5m => "Baixo peso",
            < 25m => "Peso adequado",
            < 30m => "Sobrepeso",
            < 35m => "Obesidade grau I",
            < 40m => "Obesidade grau II",
            _ => "Obesidade grau III"
        };
    }

    public static decimal? CalcularPesoIdealLorentz(decimal? alturaCm, Sexo? sexo)
    {
        if (alturaCm is null or <= 0 || sexo is null)
        {
            return null;
        }

        var altura = alturaCm.Value;
        var peso = sexo == Sexo.Masculino
            ? altura - 100m - ((altura - 150m) / 4m)
            : altura - 100m - ((altura - 150m) / 2.5m);

        return peso <= 0 ? null : decimal.Round(peso, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal? CalcularRelacaoCinturaQuadril(decimal? cinturaCm, decimal? quadrilCm)
    {
        if (cinturaCm is null or <= 0 || quadrilCm is null or <= 0)
        {
            return null;
        }

        return decimal.Round(cinturaCm.Value / quadrilCm.Value, 3, MidpointRounding.AwayFromZero);
    }

    public static int? CalcularIdade(DateOnly? dataNascimento, DateOnly referencia)
    {
        if (dataNascimento is null)
        {
            return null;
        }

        var idade = referencia.Year - dataNascimento.Value.Year;
        if (referencia < dataNascimento.Value.AddYears(idade))
        {
            idade--;
        }

        return idade < 0 ? null : idade;
    }

    public static ResultadoPregas? CalcularJacksonPollock(
        ProtocoloPregaCutanea protocolo,
        Sexo sexo,
        int idade,
        decimal pesoKg,
        decimal? axilarMedia,
        decimal? triceps,
        decimal? subescapular,
        decimal? supraIliaca,
        decimal? torax,
        decimal? abdominal,
        decimal? coxa)
    {
        if (idade <= 0 || pesoKg <= 0)
        {
            return null;
        }

        decimal soma;
        if (protocolo == ProtocoloPregaCutanea.JacksonPollock7)
        {
            if (axilarMedia is null || triceps is null || subescapular is null || supraIliaca is null
                || torax is null || abdominal is null || coxa is null)
            {
                return null;
            }

            soma = axilarMedia.Value + triceps.Value + subescapular.Value + supraIliaca.Value
                + torax.Value + abdominal.Value + coxa.Value;
        }
        else
        {
            if (sexo == Sexo.Masculino)
            {
                if (torax is null || abdominal is null || coxa is null)
                {
                    return null;
                }

                soma = torax.Value + abdominal.Value + coxa.Value;
            }
            else
            {
                if (triceps is null || supraIliaca is null || coxa is null)
                {
                    return null;
                }

                soma = triceps.Value + supraIliaca.Value + coxa.Value;
            }
        }

        var densidade = protocolo == ProtocoloPregaCutanea.JacksonPollock7
            ? CalcularDensidadeJacksonPollock7(sexo, soma, idade)
            : CalcularDensidadeJacksonPollock3(sexo, soma, idade);

        if (densidade <= 0)
        {
            return null;
        }

        var percentualGorda = decimal.Round((495m / densidade) - 450m, 2, MidpointRounding.AwayFromZero);
        if (percentualGorda < 0)
        {
            percentualGorda = 0;
        }

        var massaGorda = decimal.Round(pesoKg * percentualGorda / 100m, 2, MidpointRounding.AwayFromZero);
        var percentualMagra = decimal.Round(100m - percentualGorda, 2, MidpointRounding.AwayFromZero);
        var massaMagra = decimal.Round(pesoKg - massaGorda, 2, MidpointRounding.AwayFromZero);

        return new ResultadoPregas(percentualGorda, massaGorda, percentualMagra, massaMagra, soma, densidade);
    }

    private static decimal CalcularDensidadeJacksonPollock7(Sexo sexo, decimal soma, int idade)
    {
        var s = soma;
        var s2 = soma * soma;

        if (sexo == Sexo.Masculino)
        {
            return 1.112m - (0.00043499m * s) + (0.00000055m * s2) - (0.00028826m * idade);
        }

        return 1.097m - (0.00046971m * s) + (0.00000056m * s2) - (0.00012828m * idade);
    }

    private static decimal CalcularDensidadeJacksonPollock3(Sexo sexo, decimal soma, int idade)
    {
        var s = soma;
        var s2 = soma * soma;

        if (sexo == Sexo.Masculino)
        {
            return 1.10938m - (0.0008267m * s) + (0.0000016m * s2) - (0.0002574m * idade);
        }

        return 1.0994921m - (0.0009929m * s) + (0.0000023m * s2) - (0.0001392m * idade);
    }
}

public sealed record ResultadoPregas(
    decimal PercentualMassaGorda,
    decimal MassaGordaKg,
    decimal PercentualMassaMagra,
    decimal MassaMagraKg,
    decimal SomaPregas,
    decimal DensidadeCorporal);
