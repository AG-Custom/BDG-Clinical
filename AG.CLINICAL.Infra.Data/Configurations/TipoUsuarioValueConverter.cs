using AG.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AG.CLINICAL.Infra.Data.Configurations;

internal sealed class TipoUsuarioValueConverter : ValueConverter<TipoUsuario, string>
{
    public TipoUsuarioValueConverter()
        : base(
            tipoUsuario => tipoUsuario.ToString(),
            value => Parse(value))
    {
    }

    private static TipoUsuario Parse(string value)
    {
        return value switch
        {
            "Admin" or "ADMIN" => TipoUsuario.Admin,
            "Funcionario" or "FUNCIONARIO" => TipoUsuario.Funcionario,
            "Usuario" or "USUARIO" => TipoUsuario.Funcionario,
            _ => throw new InvalidOperationException(
                $"Valor '{value}' não é válido para o enum {nameof(TipoUsuario)}.")
        };
    }
}
