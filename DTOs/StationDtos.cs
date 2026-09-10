using System.ComponentModel.DataAnnotations;
using APICargadores.Models;

namespace APICargadores.DTOs;

/// <summary>Respuesta estándar de un cargador hacia el cliente.</summary>
public record StationDto(
    string Id,
    string StationId,
    string Address,
    double Latitude,
    double Longitude,
    string ChargerType,
    decimal CostPerKwh,
    string Availability,
    string Operator,
    int CapacityKw,
    List<string> ConnectorTypes,
    bool RenewableEnergy,
    double Rating,
    int ParkingSpots,
    StationStatus Status,
    double? DistanceKm = null);

/// <summary>Filtros para el listado/búsqueda de cargadores (query string).</summary>
public class StationFilterDto
{
    public string? ChargerType { get; set; }
    public string? ConnectorType { get; set; }
    public int? MinCapacityKw { get; set; }
    public decimal? MaxCostPerKwh { get; set; }
    public string? Operator { get; set; }
    public bool? RenewableOnly { get; set; }
    public StationStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>Datos para crear un cargador (admin).</summary>
public record CreateStationDto(
    [Required] string StationId,
    [Required] string Address,
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude,
    [Required] string ChargerType,
    [Range(0, 100)] decimal CostPerKwh,
    string Availability,
    string Operator,
    [Range(1, 1000)] int CapacityKw,
    List<string> ConnectorTypes,
    int InstallationYear,
    bool RenewableEnergy,
    int ParkingSpots,
    string MaintenanceFrequency);

/// <summary>Datos para editar un cargador (admin). Campos opcionales = parche.</summary>
public record UpdateStationDto(
    string? Address,
    string? ChargerType,
    decimal? CostPerKwh,
    string? Availability,
    string? Operator,
    int? CapacityKw,
    List<string>? ConnectorTypes,
    bool? RenewableEnergy,
    int? ParkingSpots,
    string? MaintenanceFrequency,
    StationStatus? Status);
