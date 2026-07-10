namespace BGD.CLINICAL.Application.Patients.Dtos;

public sealed record PatientUnitDto(Guid Id, string Nome);

public sealed record PatientAddressDto(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf);

public sealed record PatientAddressRequest(
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Complemento = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Uf = null);

public sealed record PatientDto(
    Guid Id,
    Guid UnidadeId,
    IReadOnlyList<PatientUnitDto> Unidades,
    IReadOnlyList<Guid> UnidadeIds,
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    DateOnly? DataNascimento,
    PatientAddressDto? Endereco,
    string? Observacao,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreatePatientRequest(
    string Nome,
    Guid? UnidadeId = null,
    IReadOnlyList<Guid>? UnidadeIds = null,
    string? Cpf = null,
    string? Telefone = null,
    string? Email = null,
    DateOnly? DataNascimento = null,
    PatientAddressRequest? Endereco = null,
    string? Observacao = null);

public sealed record UpdatePatientRequest(
    string Nome,
    Guid? UnidadeId = null,
    IReadOnlyList<Guid>? UnidadeIds = null,
    string? Cpf = null,
    string? Telefone = null,
    string? Email = null,
    DateOnly? DataNascimento = null,
    PatientAddressRequest? Endereco = null,
    string? Observacao = null);
