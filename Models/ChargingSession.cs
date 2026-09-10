using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APICargadores.Models;

/// <summary>
/// Sesión de carga iniciada por un usuario en una estación. La energía y el
/// costo se calculan al cerrar la sesión.
/// </summary>
public class ChargingSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string StationId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Datos denormalizados para mostrar el historial sin joins.</summary>
    public string StationCode { get; set; } = string.Empty;
    public string StationAddress { get; set; } = string.Empty;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }

    public double EnergyKwh { get; set; }
    public decimal Cost { get; set; }

    /// <summary>Costo por kWh vigente al iniciar la sesión.</summary>
    public decimal CostPerKwh { get; set; }

    [BsonRepresentation(BsonType.String)]
    public SessionStatus Status { get; set; } = SessionStatus.Active;
}
