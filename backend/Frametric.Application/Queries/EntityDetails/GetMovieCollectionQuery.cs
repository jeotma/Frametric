using MediatR;

namespace Frametric.Application.Queries.EntityDetails;

public record GetMovieCollectionQuery(Guid UserId, Guid MovieId) : IRequest<MovieCollectionResult?>;

public record MovieCollectionResult(int? TmdbCollectionId, string? TmdbCollectionName);
