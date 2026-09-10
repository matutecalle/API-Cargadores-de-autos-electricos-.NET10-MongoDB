namespace APICargadores.Models;

/// <summary>Estado operativo de un cargador (lo gestiona el admin).</summary>
public enum StationStatus
{
    Available,
    InUse,
    OutOfService
}

/// <summary>Estado de una sesión de carga.</summary>
public enum SessionStatus
{
    Active,
    Completed
}

/// <summary>Roles de usuario para autorización.</summary>
public static class UserRoles
{
    public const string User = "User";
    public const string Admin = "Admin";
}
