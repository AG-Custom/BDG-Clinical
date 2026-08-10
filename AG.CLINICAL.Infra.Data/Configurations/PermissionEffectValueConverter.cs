using AG.CLINICAL.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AG.CLINICAL.Infra.Data.Configurations;

internal sealed class PermissionEffectValueConverter : ValueConverter<PermissionEffect, string>
{
    public PermissionEffectValueConverter()
        : base(
            effect => effect.ToString(),
            value => Enum.Parse<PermissionEffect>(value, ignoreCase: true))
    {
    }
}
