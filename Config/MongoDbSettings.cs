namespace APICargadores.Config;

/// <summary>
/// Configuración de la conexión a MongoDB. Se enlaza desde la sección
/// "MongoDb" de la configuración (user-secrets / appsettings).
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "EvChargingDb";
}
