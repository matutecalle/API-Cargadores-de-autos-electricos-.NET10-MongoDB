namespace APICargadores.DTOs;

/// <summary>Resumen del dashboard de administración (estado en tiempo real).</summary>
public record DashboardDto(
    long TotalStations,
    long Available,
    long InUse,
    long OutOfService,
    long ActiveSessions,
    double TotalEnergyKwh,
    decimal TotalRevenue);

/// <summary>Sesiones agrupadas por día.</summary>
public record SessionsPerDayDto(DateTime Date, long Count, double EnergyKwh);

/// <summary>Cargador más usado (ranking).</summary>
public record TopStationDto(
    string StationId,
    string StationCode,
    string Address,
    long SessionCount,
    double EnergyKwh,
    decimal Revenue);
