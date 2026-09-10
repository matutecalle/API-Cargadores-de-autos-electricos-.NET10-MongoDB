using System.ComponentModel.DataAnnotations;
using APICargadores.Models;

namespace APICargadores.DTOs;

/// <summary>Inicia una sesión de carga en una estación.</summary>
public record StartSessionDto([Required] string StationId);

/// <summary>
/// Cierra una sesión. La energía puede enviarse explícitamente; si no, el
/// servicio la estima a partir de la capacidad de la estación y la duración.
/// </summary>
public record StopSessionDto(double? EnergyKwh);

public record SessionDto(
    string Id,
    string StationId,
    string StationCode,
    string StationAddress,
    DateTime StartTime,
    DateTime? EndTime,
    double EnergyKwh,
    decimal Cost,
    SessionStatus Status);
