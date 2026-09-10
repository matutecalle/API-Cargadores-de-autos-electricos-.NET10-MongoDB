using System.Security.Claims;
using APICargadores.DTOs;
using APICargadores.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APICargadores.Controllers;

/// <summary>Sesiones de carga del conductor autenticado.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessions;

    public SessionsController(ISessionService sessions) => _sessions = sessions;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Inicia una sesión de carga en una estación disponible.</summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start(StartSessionDto dto)
    {
        try
        {
            return Ok(await _sessions.StartAsync(UserId, dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>Cierra una sesión activa y calcula energía y costo.</summary>
    [HttpPost("{id}/stop")]
    public async Task<IActionResult> Stop(string id, StopSessionDto dto)
    {
        try
        {
            return Ok(await _sessions.StopAsync(UserId, id, dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>Historial de sesiones y costos del usuario.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> History() =>
        Ok(await _sessions.GetHistoryAsync(UserId));
}
