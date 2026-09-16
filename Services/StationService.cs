using APICargadores.DTOs;
using APICargadores.Models;
using APICargadores.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace APICargadores.Services;

public class StationService : IStationService
{
    private readonly IStationRepository _stations;

    public StationService(IStationRepository stations) => _stations = stations;

    public async Task<(List<StationDto> Items, long Total)> SearchAsync(StationFilterDto filter)
    {
        var (items, total) = await _stations.FilterAsync(filter);
        return (items.Select(s => ToDto(s)).ToList(), total);
    }

    public async Task<List<StationDto>> NearbyAsync(double lat, double lng, double maxKm, int limit)
    {
        var items = await _stations.FindNearbyAsync(lat, lng, maxKm, limit);
        // Calcula la distancia aproximada (Haversine) para mostrarla al usuario.
        return items.Select(s => ToDto(s, Haversine(lat, lng, s.Latitude, s.Longitude))).ToList();
    }

    public async Task<StationDto?> GetByIdAsync(string id)
    {
        var station = await _stations.GetByIdAsync(id);
        return station is null ? null : ToDto(station);
    }

    public async Task<StationDto> CreateAsync(CreateStationDto dto)
    {
        var code = dto.StationId.Trim();
        if (await _stations.GetByStationCodeAsync(code) is not null)
            throw new InvalidOperationException($"Ya existe una estación con el código {code}.");

        var station = new Station
        {
            StationId = code,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Location = GeoJson.Point(GeoJson.Geographic(dto.Longitude, dto.Latitude)),
            ChargerType = dto.ChargerType,
            CostPerKwh = dto.CostPerKwh,
            Availability = dto.Availability,
            Operator = dto.Operator,
            CapacityKw = dto.CapacityKw,
            ConnectorTypes = dto.ConnectorTypes ?? new(),
            InstallationYear = dto.InstallationYear,
            RenewableEnergy = dto.RenewableEnergy,
            ParkingSpots = dto.ParkingSpots,
            MaintenanceFrequency = dto.MaintenanceFrequency,
            Status = StationStatus.Available
        };

        try
        {
            await _stations.CreateAsync(station);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Dos altas simultáneas con el mismo código: el índice único frena la segunda.
            throw new InvalidOperationException($"Ya existe una estación con el código {code}.");
        }
        return ToDto(station);
    }

    public async Task<StationDto?> UpdateAsync(string id, UpdateStationDto dto)
    {
        var station = await _stations.GetByIdAsync(id);
        if (station is null) return null;

        station.Address = dto.Address ?? station.Address;
        station.ChargerType = dto.ChargerType ?? station.ChargerType;
        station.CostPerKwh = dto.CostPerKwh ?? station.CostPerKwh;
        station.Availability = dto.Availability ?? station.Availability;
        station.Operator = dto.Operator ?? station.Operator;
        station.CapacityKw = dto.CapacityKw ?? station.CapacityKw;
        station.ConnectorTypes = dto.ConnectorTypes ?? station.ConnectorTypes;
        station.RenewableEnergy = dto.RenewableEnergy ?? station.RenewableEnergy;
        station.ParkingSpots = dto.ParkingSpots ?? station.ParkingSpots;
        station.MaintenanceFrequency = dto.MaintenanceFrequency ?? station.MaintenanceFrequency;
        station.Status = dto.Status ?? station.Status;

        await _stations.ReplaceAsync(id, station);
        return ToDto(station);
    }

    public Task<bool> DeleteAsync(string id) => _stations.DeleteAsync(id);

    private static StationDto ToDto(Station s, double? distanceKm = null) => new(
        s.Id!, s.StationId, s.Address, s.Latitude, s.Longitude, s.ChargerType,
        s.CostPerKwh, s.Availability, s.Operator, s.CapacityKw, s.ConnectorTypes,
        s.RenewableEnergy, s.Rating, s.ParkingSpots, s.Status, distanceKm);

    /// <summary>Distancia en km entre dos coordenadas (fórmula de Haversine).</summary>
    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371; // radio terrestre en km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return Math.Round(r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), 2);
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
