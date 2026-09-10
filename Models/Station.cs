using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace APICargadores.Models;

/// <summary>
/// Cargador eléctrico. Mapea los campos del dataset y añade un punto GeoJSON
/// (con índice 2dsphere) para búsquedas geoespaciales y un estado operativo.
/// </summary>
public class Station
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>Identificador del dataset, ej. "EVS00001".</summary>
    public string StationId { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    /// <summary>Punto GeoJSON [longitud, latitud] para el índice 2dsphere.</summary>
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; } = default!;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>Tipo de cargador: "AC Level 1", "AC Level 2", "DC Fast Charger".</summary>
    public string ChargerType { get; set; } = string.Empty;

    public decimal CostPerKwh { get; set; }

    /// <summary>Horario de disponibilidad declarado, ej. "24/7" o "9:00-18:00".</summary>
    public string Availability { get; set; } = string.Empty;

    public double DistanceToCityKm { get; set; }
    public int AvgUsersPerDay { get; set; }
    public string Operator { get; set; } = string.Empty;
    public int CapacityKw { get; set; }

    /// <summary>Conectores soportados, ej. ["CCS", "CHAdeMO"].</summary>
    public List<string> ConnectorTypes { get; set; } = new();

    public int InstallationYear { get; set; }
    public bool RenewableEnergy { get; set; }
    public double Rating { get; set; }
    public int ParkingSpots { get; set; }
    public string MaintenanceFrequency { get; set; } = string.Empty;

    /// <summary>Estado operativo gestionado por el admin.</summary>
    [BsonRepresentation(BsonType.String)]
    public StationStatus Status { get; set; } = StationStatus.Available;
}
