// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.Imports;
using Frametric.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frametric.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly IImportApplication _importApplication;
    private readonly ICurrentUserService _currentUserService;

    public ImportController(IImportApplication importApplication, ICurrentUserService currentUserService)
    {
        _importApplication = importApplication;
        _currentUserService = currentUserService;
    }

    [HttpPost("letterboxd")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ImportLetterboxdArchive(IFormFile? file, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("A valid zip file is required.");
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .zip files are supported.");
        }

        using var stream = file.OpenReadStream();
        var importId = await _importApplication.ImportLetterboxdAsync(userId.Value, stream, cancellationToken);
        
        return Ok(new { success = true, importId = importId, message = "Archive imported successfully and enrichment has started." });
    }

    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ImportHistoryDto>>> GetImportHistory(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        var result = await _importApplication.GetImportHistoryAsync(userId.Value, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteImport(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        try
        {
            await _importApplication.DeleteImportAsync(userId.Value, id, cancellationToken);
            return Ok(new { success = true, message = "Import data successfully deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
