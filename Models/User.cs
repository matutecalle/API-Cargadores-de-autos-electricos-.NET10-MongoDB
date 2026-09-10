using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APICargadores.Models;

/// <summary>Usuario de la plataforma. La contraseña se guarda hasheada con BCrypt.</summary>
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>"User" o "Admin" (ver <see cref="UserRoles"/>).</summary>
    public string Role { get; set; } = UserRoles.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
