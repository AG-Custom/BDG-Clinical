namespace AG.CLINICAL.Application.Identity;

public static class IdentityConstants
{
    public const string AuthProviderLocal = "LOCAL";

    public const string ClaimEmpresaId = "empresa_id";

    public const string ClaimTipoUsuario = "tipo_usuario";

    public const string PolicyAdmin = "AdminOnly";

    public const string CredenciaisInvalidas = "Credenciais inválidas";

    public const string MultiplasContas = "Existem múltiplas contas com este e-mail. Entre em contato com o suporte.";

    public const string PrimeiroAcessoPendente = "É necessário definir a senha no primeiro acesso.";

    public const string ConviteInvalido = "Convite inválido ou expirado.";

    public const int SenhaMinimaCaracteres = 8;
}
