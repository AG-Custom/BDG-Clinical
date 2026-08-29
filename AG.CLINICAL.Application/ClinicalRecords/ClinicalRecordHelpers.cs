using System.Text.Json;
using AG.CLINICAL.Application.ClinicalRecords.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.ClinicalRecords;

internal static class ClinicalRecordAudit
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Summary(object value) => JsonSerializer.Serialize(value, Options);
}

internal static class ClinicalRecordEnums
{
    public static T Parse<T>(string? value, string error) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value) || !Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var parsed))
        {
            throw new DomainException(error);
        }

        return parsed;
    }

    public static T? ParseOptional<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var parsed) ? parsed : null;
    }
}

internal sealed class ClinicalRecordContextHelper
{
    private readonly IMedicalRecordsRepository _medicalRecordsRepository;
    private readonly IClinicalEncountersRepository _encountersRepository;
    private readonly IAuditLogsService _auditLogsService;

    public ClinicalRecordContextHelper(
        IMedicalRecordsRepository medicalRecordsRepository,
        IClinicalEncountersRepository encountersRepository,
        IAuditLogsService auditLogsService)
    {
        _medicalRecordsRepository = medicalRecordsRepository;
        _encountersRepository = encountersRepository;
        _auditLogsService = auditLogsService;
    }

    public async Task RegisterAsync(
        Guid empresaId,
        Guid usuarioId,
        string entidade,
        Guid registroId,
        AcaoAuditoria acao,
        CancellationToken cancellationToken,
        string? dadosAnteriores = null,
        string? dadosNovos = null)
    {
        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            usuarioId,
            entidade,
            registroId,
            acao,
            dadosAnteriores,
            dadosNovos,
            cancellationToken);
    }

    public async Task AddEventoAsync(
        Guid empresaId,
        Guid pacienteId,
        Guid? atendimentoId,
        TipoEventoClinico tipo,
        string titulo,
        string? resumo,
        string entidade,
        Guid registroId,
        Guid funcionarioId,
        Guid unidadeId,
        CancellationToken cancellationToken,
        string? dadosAnteriores = null,
        string? dadosNovos = null)
    {
        var evento = EventoClinico.Create(
            empresaId,
            pacienteId,
            atendimentoId,
            tipo,
            titulo,
            resumo,
            entidade,
            registroId,
            funcionarioId,
            unidadeId,
            dadosAnteriores,
            dadosNovos);

        await _medicalRecordsRepository.AddEventoAsync(evento, cancellationToken);
    }

    public Task<AtendimentoClinico?> GetEncounterAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        return _encountersRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);
    }
}
