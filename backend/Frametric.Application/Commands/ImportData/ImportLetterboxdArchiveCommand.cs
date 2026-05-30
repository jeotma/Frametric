using MediatR;

namespace Frametric.Application.Commands.ImportData;

public record ImportLetterboxdArchiveCommand(Guid UserId, Stream ZipStream) : IRequest<Guid>;
