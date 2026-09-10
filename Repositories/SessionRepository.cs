using System.Globalization;
using APICargadores.Config;
using APICargadores.DTOs;
using APICargadores.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace APICargadores.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly IMongoCollection<ChargingSession> _sessions;

    public SessionRepository(MongoDbContext context) => _sessions = context.Sessions;

    public async Task<ChargingSession> CreateAsync(ChargingSession session)
    {
        await _sessions.InsertOneAsync(session);
        return session;
    }

    public async Task<ChargingSession?> GetByIdAsync(string id) =>
        await _sessions.Find(s => s.Id == id).FirstOrDefaultAsync();

    public async Task<ChargingSession?> GetActiveByStationAsync(string stationId) =>
        await _sessions.Find(s => s.StationId == stationId && s.Status == SessionStatus.Active)
            .FirstOrDefaultAsync();

    public async Task<List<ChargingSession>> GetByUserAsync(string userId) =>
        await _sessions.Find(s => s.UserId == userId)
            .SortByDescending(s => s.StartTime)
            .ToListAsync();

    public async Task<bool> ReplaceAsync(string id, ChargingSession session)
    {
        var result = await _sessions.ReplaceOneAsync(s => s.Id == id, session);
        return result.ModifiedCount > 0;
    }

    public async Task<long> CountByStatusAsync(SessionStatus status) =>
        await _sessions.CountDocumentsAsync(s => s.Status == status);

    /// <summary>Suma de energía y recaudación sobre sesiones completadas.</summary>
    public async Task<(double Energy, decimal Revenue)> GetTotalsAsync()
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("Status", "Completed")),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "energy", new BsonDocument("$sum", "$EnergyKwh") },
                { "revenue", new BsonDocument("$sum", "$Cost") }
            })
        };

        var doc = await _sessions.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        if (doc is null) return (0, 0);

        return (doc["energy"].ToDouble(), doc["revenue"].ToDecimal());
    }

    /// <summary>Sesiones agrupadas por día (últimos N días), ordenadas por fecha.</summary>
    public async Task<List<SessionsPerDayDto>> GetSessionsPerDayAsync(int days)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days + 1);
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "Status", "Completed" },
                { "StartTime", new BsonDocument("$gte", since) }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument("$dateToString",
                    new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$StartTime" } }) },
                { "count", new BsonDocument("$sum", 1) },
                { "energy", new BsonDocument("$sum", "$EnergyKwh") }
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var docs = await _sessions.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return docs.Select(d => new SessionsPerDayDto(
            DateTime.ParseExact(d["_id"].AsString, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            d["count"].ToInt64(),
            d["energy"].ToDouble())).ToList();
    }

    /// <summary>Ranking de cargadores más usados por número de sesiones.</summary>
    public async Task<List<TopStationDto>> GetTopStationsAsync(int limit)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("Status", "Completed")),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$StationId" },
                { "code", new BsonDocument("$first", "$StationCode") },
                { "address", new BsonDocument("$first", "$StationAddress") },
                { "count", new BsonDocument("$sum", 1) },
                { "energy", new BsonDocument("$sum", "$EnergyKwh") },
                { "revenue", new BsonDocument("$sum", "$Cost") }
            }),
            new BsonDocument("$sort", new BsonDocument("count", -1)),
            new BsonDocument("$limit", limit)
        };

        var docs = await _sessions.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return docs.Select(d => new TopStationDto(
            d["_id"].AsObjectId.ToString(),
            d.GetValue("code", "").AsString,
            d.GetValue("address", "").AsString,
            d["count"].ToInt64(),
            d["energy"].ToDouble(),
            d["revenue"].ToDecimal())).ToList();
    }
}
