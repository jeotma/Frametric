namespace Frametric.Domain.ValueObjects;

public class ExternalReference : IEquatable<ExternalReference>
{
    public string Source { get; } = null!;
    public string ExternalId { get; } = null!;

    private ExternalReference() { } // For EF Core

    public ExternalReference(string source, string externalId)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be empty", nameof(source));
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("ExternalId cannot be empty", nameof(externalId));

        Source = source;
        ExternalId = externalId;
    }

    public bool Equals(ExternalReference? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Source == other.Source && ExternalId == other.ExternalId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ExternalReference)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source, ExternalId);
    }
}
