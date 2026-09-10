using APICargadores.Config;
using APICargadores.DTOs;
using APICargadores.Models;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace APICargadores.Repositories;

public class StationRepository : IStationRepository
{
    private readonly IMongoCollection<Station> _stations;

    public StationRepository(MongoDbContext context) => _stations = context.Stations;

    public async Task<(List<Station> Items, long Total)> FilterAsync(StationFilterDto filter)
    {
        var builder = Builders<Station>.Filter;
        var conditions = new List<FilterDefinition<Station>>();

        if (!string.IsNullOrWhiteSpace(filter.ChargerType))
            conditions.Add(builder.Eq(s => s.ChargerType, filter.ChargerType));

        if (!string.IsNullOrWhiteSpace(filter.ConnectorType))
            conditions.Add(builder.AnyEq(s => s.ConnectorTypes, filter.ConnectorType));

        if (filter.MinCapacityKw.HasValue)
            conditions.Add(builder.Gte(s => s.CapacityKw, filter.MinCapacityKw.Value));

        if (filter.MaxCostPerKwh.HasValue)
            conditions.Add(builder.Lte(s => s.CostPerKwh, filter.MaxCostPerKwh.Value));

        if (!string.IsNullOrWhiteSpace(filter.Operator))
            conditions.Add(builder.Eq(s => s.Operator, filter.Operator));

        if (filter.RenewableOnly == true)
            conditions.Add(builder.Eq(s => s.RenewableEnergy, true));

        if (filter.Status.HasValue)
            conditions.Add(builder.Eq(s => s.Status, filter.Status.Value));

        var combined = conditions.Count > 0 ? builder.And(conditions) : builder.Empty;

        var total = await _stations.CountDocumentsAsync(combined);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 100 ? 20 : filter.PageSize;

        var items = await _stations.Find(combined)
            .Skip((page - 1) * size)
            .Limit(size)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Station>> FindNearbyAsync(double latitude, double longitude, double maxKm, int limit)
    {
        var point = GeoJson.Point(GeoJson.Geographic(longitude, latitude));
        // $near devuelve los documentos ordenados por distancia ascendente.
        var filter = Builders<Station>.Filter.Near(s => s.Location, point, maxDistance: maxKm * 1000);
        return await _stations.Find(filter).Limit(limit).ToListAsync();
    }

    public async Task<Station?> GetByIdAsync(string id) =>
        await _stations.Find(s => s.Id == id).FirstOrDefaultAsync();

    public async Task<Station?> GetByStationCodeAsync(string stationCode) =>
        await _stations.Find(s => s.StationId == stationCode).FirstOrDefaultAsync();

    public async Task<Station> CreateAsync(Station station)
    {
        await _stations.InsertOneAsync(station);
        return station;
    }

    public async Task<bool> ReplaceAsync(string id, Station station)
    {
        var result = await _stations.ReplaceOneAsync(s => s.Id == id, station);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _stations.DeleteOneAsync(s => s.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<long> CountAsync() =>
        await _stations.CountDocumentsAsync(Builders<Station>.Filter.Empty);

    public async Task<long> CountByStatusAsync(StationStatus status) =>
        await _stations.CountDocumentsAsync(s => s.Status == status);
}
