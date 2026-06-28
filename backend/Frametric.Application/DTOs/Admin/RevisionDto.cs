using System;

namespace Frametric.Application.DTOs.Admin;

public record RevisionDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    DateTime ChangedAt,
    string ChangedBy,
    string StateJson);
