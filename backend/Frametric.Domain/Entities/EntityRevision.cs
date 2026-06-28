using System;

namespace Frametric.Domain.Entities;

public class EntityRevision
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string ChangedBy { get; private set; } = null!;
    public string StateJson { get; private set; } = null!;

    private EntityRevision() { }

    public EntityRevision(Guid id, string entityType, Guid entityId, string changedBy, string stateJson)
    {
        Id = id;
        EntityType = entityType;
        EntityId = entityId;
        ChangedAt = DateTime.UtcNow;
        ChangedBy = changedBy;
        StateJson = stateJson;
    }
}
