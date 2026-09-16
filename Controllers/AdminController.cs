using APICargadores.DTOs;
using APICargadores.Models;
using APICargadores.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APICargadores.Controllers;

/// <summary>Panel de administración: monitoreo, estadísticas y gestión de cargadores.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IStatsService _stats;
    private readonly IStationService _stations;

    public AdminController(IStatsService stats, IStationService stations)
    {
        _stats = stats;
        _stations = stations;
    }

    // ----- Monitoreo y estadísticas (aggregation) -----

    /// <summary>Estado en tiempo real: conteos por estado, sesiones activas, totales.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard() =>
        Ok(await _stats.GetDashboardAsync());

    /// <summary>Sesiones y energía por día (últimos N días).</summary>
    [HttpGet("stats/sessions-per-day")]
    public async Task<IActionResult> SessionsPerDay([FromQuery] int days = 7) =>
        Ok(await _stats.GetSessionsPerDayAsync(days));

    /// <summary>Cargadores más usados.</summary>
    [HttpGet("stats/top-stations")]
    public async Task<IActionResult> TopStations([FromQuery] int limit = 10) =>
        Ok(await _stats.GetTopStationsAsync(limit));

    // ----- Gestión de cargadores (CRUD) -----

    /// <summary>Crea un nuevo cargador.</summary>
    [HttpPost("stations")]
    public async Task<IActionResult> Create(CreateStationDto dto)
    {
        try
        {
            var created = await _stations.CreateAsync(dto);
            return CreatedAtAction(nameof(StationsController.GetById),
                "Stations", new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>Edita un cargador (parcial).</summary>
    [HttpPut("stations/{id}")]
    public async Task<IActionResult> Update(string id, UpdateStationDto dto)
    {
        var updated = await _stations.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Marca un cargador como fuera de servicio.</summary>
    [HttpPatch("stations/{id}/out-of-service")]
    public async Task<IActionResult> MarkOutOfService(string id)
    {
        var updated = await _stations.UpdateAsync(id,
            new UpdateStationDto(null, null, null, null, null, null, null, null, null, null,
                StationStatus.OutOfService));
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Elimina un cargador.</summary>
    [HttpDelete("stations/{id}")]
    public async Task<IActionResult> Delete(string id) =>
        await _stations.DeleteAsync(id) ? NoContent() : NotFound();
}
