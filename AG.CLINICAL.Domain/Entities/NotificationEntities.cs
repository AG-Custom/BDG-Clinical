using AG.CLINICAL.Domain.Common;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Domain.Entities;

public sealed class OutputMessageEmail : AggregateRoot
{
    public const int MaxRetryAttempts = 5;

    private OutputMessageEmail()
    {
    }

    public OutputMessageEmail(
        Guid empresaId,
        Guid? unidadeId,
        EmailMessageType tipo,
        string destinatarioEmail,
        string? destinatarioNome,
        string payloadJson)
        : base(Guid.NewGuid())
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa da mensagem de e-mail.");
        }

        if (string.IsNullOrWhiteSpace(destinatarioEmail))
        {
            throw new DomainException("Informe o destinatário do e-mail.");
        }

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new DomainException("Informe o conteúdo da mensagem de e-mail.");
        }

        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        Tipo = tipo;
        DestinatarioEmail = destinatarioEmail.Trim();
        DestinatarioNome = string.IsNullOrWhiteSpace(destinatarioNome) ? null : destinatarioNome.Trim();
        PayloadJson = payloadJson;
        Status = EmailOutboxStatus.Pendente;
        Tentativas = 0;
        CriadoEm = DateTime.UtcNow;
    }

    public Guid EmpresaId { get; private set; }
    public Guid? UnidadeId { get; private set; }
    public EmailMessageType Tipo { get; private set; }
    public string DestinatarioEmail { get; private set; } = string.Empty;
    public string? DestinatarioNome { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public EmailOutboxStatus Status { get; private set; }
    public int Tentativas { get; private set; }
    public string? Erro { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }

    public void MarkAsProcessing()
    {
        Status = EmailOutboxStatus.Processando;
        Tentativas++;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        Status = EmailOutboxStatus.Enviado;
        ProcessadoEm = DateTime.UtcNow;
        Erro = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarkAsFailed(string erro)
    {
        Erro = string.IsNullOrWhiteSpace(erro) ? "Falha desconhecida no envio." : erro.Trim();

        if (Tentativas >= MaxRetryAttempts)
        {
            Status = EmailOutboxStatus.Falhou;
        }
        else
        {
            Status = EmailOutboxStatus.Pendente;
        }

        AtualizadoEm = DateTime.UtcNow;
    }
}
