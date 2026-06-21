namespace BGD.CLINICAL.Domain.Common;

public abstract class Entity
{
    private readonly List<object> _domainEvents = [];

    protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        Id = id;
        CriadoEm = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; }

    public DateTime CriadoEm { get; protected set; }

    public DateTime? AtualizadoEm { get; protected set; }

    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id != Guid.Empty && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    protected void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
