using MediatR;

namespace Frametric.Application.Commands.EnrichMovies;

public record EnrichPendingMoviesCommand(int BatchSize) : IRequest<int>;
