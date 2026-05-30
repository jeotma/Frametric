using Frametric.Application.DTOs.Imports;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.Imports;

public record GetImportHistoryQuery(Guid UserId) : IRequest<List<ImportHistoryDto>>;

public class GetImportHistoryQueryHandler : IRequestHandler<GetImportHistoryQuery, List<ImportHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetImportHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ImportHistoryDto>> Handle(GetImportHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _context.ImportHistories
            .Where(ih => ih.UserId == request.UserId)
            .OrderByDescending(ih => ih.ImportDate)
            .Select(ih => new ImportHistoryDto(
                ih.Id,
                ih.ImportDate,
                ih.RowCount,
                ih.Status,
                ih.ProviderSource,
                ih.ErrorMessage
            ))
            .ToListAsync(cancellationToken);
    }
}
