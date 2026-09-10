using APICargadores.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace APICargadores.Config;

/// <summary>
/// Punto único de acceso a la base de datos. Expone las colecciones tipadas y
/// crea los índices necesarios (2dsphere para geo, único para email).
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
        EnsureIndexes();
    }

    public IMongoCollection<Station> Stations => _database.GetCollection<Station>("stations");
    public IMongoCollection<ChargingSession> Sessions => _database.GetCollection<ChargingSession>("sessions");
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");

    private void EnsureIndexes()
    {
        // Índice geoespacial para búsquedas "cargadores cercanos" ($near).
        Stations.Indexes.CreateOne(new CreateIndexModel<Station>(
            Builders<Station>.IndexKeys.Geo2DSphere(s => s.Location)));

        // Email único para autenticación.
        Users.Indexes.CreateOne(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true }));

        // Acelera el historial por usuario y las stats por estación.
        Sessions.Indexes.CreateOne(new CreateIndexModel<ChargingSession>(
            Builders<ChargingSession>.IndexKeys.Ascending(s => s.UserId)));
        Sessions.Indexes.CreateOne(new CreateIndexModel<ChargingSession>(
            Builders<ChargingSession>.IndexKeys.Ascending(s => s.StationId)));
    }
}
