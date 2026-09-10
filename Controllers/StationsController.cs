using APICargadores.DTOs;
using APICargadores.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APICargadores.Controllers;

/// <summary>Endpoints de consulta de cargadores para el conductor de EV.</summary>
[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IStationService _stations;

    public StationsController(IStationService stations) => _stations = stations;

    /// <summary>Lista/búsqueda con filtros (tipo, conector, potencia, costo, estado).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] StationFilterDto filter)
    {
        var (items, total) = await _stations.SearchAsync(filter);
        return Ok(new { total, filter.Page, filter.PageSize, items });
    }

    /// <summary>Cargadores cercanos a una coordenada (índice geoespacial 2dsphere).</summary>
    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<IActionResult> Nearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double maxKm = 10,
        [FromQuery] int limit = 20)
    {
        var items = await _stations.NearbyAsync(lat, lng, maxKm, limit);
        return Ok(items);
    }

    /// <summary>Detalle de un cargador.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id)
    {
        var station = await _stations.GetByIdAsync(id);
        return station is null ? NotFound() : Ok(station);
    }
}
