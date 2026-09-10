using System.Globalization;
using APICargadores.Config;
using APICargadores.Models;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace APICargadores.Services;

/// <summary>
/// Siembra la base la primera vez que arranca la app: importa el dataset CSV
/// de estaciones y crea un usuario administrador por defecto.
/// </summary>
public class CsvSeeder
{
    private readonly MongoDbContext _context;
    private readonly ILogger<CsvSeeder> _logger;
    private readonly IWebHostEnvironment _env;

    public CsvSeeder(MongoDbContext context, ILogger<CsvSeeder> logger, IWebHostEnvironment env)
    {
        _context = context;
        _logger = logger;
        _env = env;
    }

    public async Task SeedAsync()
    {
        await SeedAdminAsync();
        await SeedStationsAsync();
    }

    private async Task SeedAdminAsync()
    {
        const string adminEmail = "admin@cargadores.com";
        var exists = await _context.Users.Find(u => u.Email == adminEmail).AnyAsync();
        if (exists) return;

        await _context.Users.InsertOneAsync(new User
        {
            Email = adminEmail,
            DisplayName = "Administrador",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRoles.Admin
        });
        _logger.LogInformation("Usuario admin creado: {Email} / Admin123!", adminEmail);
    }

    private async Task SeedStationsAsync()
    {
        if (await _context.Stations.EstimatedDocumentCountAsync() > 0)
        {
            _logger.LogInformation("Las estaciones ya están sembradas. Se omite la importación.");
            return;
        }

        var path = Path.Combine(_env.ContentRootPath, "Data", "stations.csv");
        if (!File.Exists(path))
        {
            _logger.LogWarning("No se encontró el CSV en {Path}. Se omite la siembra.", path);
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null };
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        var stations = csv.GetRecords<CsvStationRow>().Select(MapRow).ToList();
        await _context.Stations.InsertManyAsync(stations);
        _logger.LogInformation("Se importaron {Count} estaciones desde el CSV.", stations.Count);
    }

    private static Station MapRow(CsvStationRow r) => new()
    {
        StationId = r.StationId,
        Address = r.Address,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        Location = GeoJson.Point(GeoJson.Geographic(r.Longitude, r.Latitude)),
        ChargerType = r.ChargerType,
        CostPerKwh = r.CostPerKwh,
        Availability = r.Availability,
        DistanceToCityKm = r.DistanceToCityKm,
        AvgUsersPerDay = r.AvgUsersPerDay,
        Operator = r.Operator,
        CapacityKw = r.CapacityKw,
        ConnectorTypes = (r.ConnectorTypes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList(),
        InstallationYear = r.InstallationYear,
        RenewableEnergy = string.Equals(r.RenewableEnergy, "Yes", StringComparison.OrdinalIgnoreCase),
        Rating = r.Rating,
        ParkingSpots = r.ParkingSpots,
        MaintenanceFrequency = r.MaintenanceFrequency,
        Status = StationStatus.Available
    };

    /// <summary>Fila cruda del CSV; los nombres mapean los encabezados del dataset.</summary>
    private sealed class CsvStationRow
    {
        [Name("Station ID")] public string StationId { get; set; } = string.Empty;
        [Name("Latitude")] public double Latitude { get; set; }
        [Name("Longitude")] public double Longitude { get; set; }
        [Name("Address")] public string Address { get; set; } = string.Empty;
        [Name("Charger Type")] public string ChargerType { get; set; } = string.Empty;
        [Name("Cost (USD/kWh)")] public decimal CostPerKwh { get; set; }
        [Name("Availability")] public string Availability { get; set; } = string.Empty;
        [Name("Distance to City (km)")] public double DistanceToCityKm { get; set; }
        [Name("Usage Stats (avg users/day)")] public int AvgUsersPerDay { get; set; }
        [Name("Station Operator")] public string Operator { get; set; } = string.Empty;
        [Name("Charging Capacity (kW)")] public int CapacityKw { get; set; }
        [Name("Connector Types")] public string? ConnectorTypes { get; set; }
        [Name("Installation Year")] public int InstallationYear { get; set; }
        [Name("Renewable Energy Source")] public string RenewableEnergy { get; set; } = "No";
        [Name("Reviews (Rating)")] public double Rating { get; set; }
        [Name("Parking Spots")] public int ParkingSpots { get; set; }
        [Name("Maintenance Frequency")] public string MaintenanceFrequency { get; set; } = string.Empty;
    }
}
