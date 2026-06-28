using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record RestoreRevisionCommand(Guid RevisionId) : IRequest<bool>;

public class RestoreRevisionCommandHandler : IRequestHandler<RestoreRevisionCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RestoreRevisionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(RestoreRevisionCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        if (actor.Role != UserRole.SuperAdmin && !(actor.Role == UserRole.Admin && actor.CanManageCatalog))
        {
            throw new UnauthorizedAccessException("Insufficient privileges to manage the catalog.");
        }

        var revision = await _context.EntityRevisions.FirstOrDefaultAsync(r => r.Id == request.RevisionId, cancellationToken);
        if (revision == null) return false;

        object? entity = null;
        if (revision.EntityType == "Movie")
        {
            entity = await _context.Movies.FirstOrDefaultAsync(m => m.Id == revision.EntityId, cancellationToken);
        }
        else if (revision.EntityType == "Actor")
        {
            entity = await _context.Actors.FirstOrDefaultAsync(a => a.Id == revision.EntityId, cancellationToken);
        }
        else if (revision.EntityType == "Director")
        {
            entity = await _context.Directors.FirstOrDefaultAsync(d => d.Id == revision.EntityId, cancellationToken);
        }
        else if (revision.EntityType == "User")
        {
            entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == revision.EntityId, cancellationToken);
        }

        if (entity == null) return false;

        var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(revision.StateJson);
        if (values == null) return false;

        var entry = ((DbContext)_context).Entry(entity);
        foreach (var kv in values)
        {
            var prop = entry.Metadata.FindProperty(kv.Key);
            if (prop != null && !prop.IsKey() && kv.Value != null)
            {
                if (kv.Value is JsonElement element)
                {
                    var propType = prop.ClrType;
                    var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;
                    
                    object? convertedValue = null;
                    try
                    {
                        if (underlyingType == typeof(Guid)) convertedValue = element.GetGuid();
                        else if (underlyingType == typeof(int)) convertedValue = element.GetInt32();
                        else if (underlyingType == typeof(double)) convertedValue = element.GetDouble();
                        else if (underlyingType == typeof(bool)) convertedValue = element.GetBoolean();
                        else if (underlyingType == typeof(string)) convertedValue = element.GetString();
                        else if (underlyingType == typeof(DateTime)) convertedValue = element.GetDateTime();
                        else convertedValue = JsonSerializer.Deserialize(element.GetRawText(), underlyingType);
                    }
                    catch
                    {
                        // Fallback
                        continue;
                    }

                    entry.Property(kv.Key).CurrentValue = convertedValue;
                }
                else
                {
                    entry.Property(kv.Key).CurrentValue = kv.Value;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
