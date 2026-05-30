using Frametric.Application.Commands.ImportData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("letterboxd")]
    public async Task<IActionResult> ImportLetterboxdArchive(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("A valid zip file is required.");
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .zip files are supported.");
        }

        // Simulating an authenticated user for the MVP
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        using var stream = file.OpenReadStream();
        var command = new ImportLetterboxdArchiveCommand(userId, stream);
        
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(new { success = result, message = "Archive imported successfully." });
    }
}
