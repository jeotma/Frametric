using Frametric.Application.Commands.CustomLists;
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.CustomLists;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/v1/custom-lists")]
[Authorize]
public class CustomListsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public CustomListsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    private Guid GetUserIdOrThrow()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomListDto>>> GetUserLists(CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var result = await _mediator.Send(new GetUserCustomListsQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomListDto>> CreateList([FromBody] CreateCustomListRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var result = await _mediator.Send(new CreateCustomListCommand(userId, request.Name, request.MovieIds), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteList(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var success = await _mediator.Send(new DeleteCustomListCommand(userId, id), cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}/movies/{movieId}")]
    public async Task<IActionResult> RemoveMovieFromList(Guid id, Guid movieId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var success = await _mediator.Send(new RemoveMovieFromCustomListCommand(userId, id, movieId), cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }
}

public class CreateCustomListRequest
{
    public string Name { get; set; } = string.Empty;
    public List<Guid> MovieIds { get; set; } = new();
}
