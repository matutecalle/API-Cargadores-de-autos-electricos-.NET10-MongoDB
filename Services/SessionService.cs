using APICargadores.DTOs;
using APICargadores.Models;
using APICargadores.Repositories;

namespace APICargadores.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessions;
    private readonly IStationRepository _stations;

    public SessionService(ISessionRepository sessions, IStationRepository stations)
    {
        _sessions = sessions;
        _stations = stations;
    }

    public async Task<SessionDto> StartAsync(string userId, StartSessionDto dto)
    {
        var station = await _stations.GetByIdAsync(dto.StationId)
            ?? throw new KeyNotFoundException("La estación no existe.");

        if (station.Status == StationStatus.OutOfService)
            throw new InvalidOperationException("La estación está fuera de servicio.");

        if (await _sessions.GetActiveByStationAsync(station.Id!) is not null)
            throw new InvalidOperationException("La estación ya tiene una sesión activa.");

        var session = new ChargingSession
        {
            StationId = station.Id!,
            UserId = userId,
            StationCode = station.StationId,
            StationAddress = station.Address,
            CostPerKwh = station.CostPerKwh,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active
        };
        await _sessions.CreateAsync(session);

        // La estación pasa a "en uso" para el dashboard en tiempo real.
        station.Status = StationStatus.InUse;
        await _stations.ReplaceAsync(station.Id!, station);

        return ToDto(session);
    }

    public async Task<SessionDto> StopAsync(string userId, string sessionId, StopSessionDto dto)
    {
        var session = await _sessions.GetByIdAsync(sessionId)
            ?? throw new KeyNotFoundException("La sesión no existe.");

        if (session.UserId != userId)
            throw new UnauthorizedAccessException("La sesión no pertenece al usuario.");

        if (session.Status == SessionStatus.Completed)
            throw new InvalidOperationException("La sesión ya está cerrada.");

        session.EndTime = DateTime.UtcNow;

        // Si el cliente no envía energía, se estima a partir de la capacidad
        // de la estación y la duración (factor 0.5 por carga parcial).
        var station = await _stations.GetByIdAsync(session.StationId);
        if (dto.EnergyKwh is > 0)
        {
            session.EnergyKwh = dto.EnergyKwh.Value;
        }
        else
        {
            var hours = (session.EndTime.Value - session.StartTime).TotalHours;
            var capacity = station?.CapacityKw ?? 50;
            session.EnergyKwh = Math.Round(Math.Max(0.1, capacity * hours * 0.5), 2);
        }

        session.Cost = Math.Round(session.CostPerKwh * (decimal)session.EnergyKwh, 2);
        session.Status = SessionStatus.Completed;
        await _sessions.ReplaceAsync(session.Id!, session);

        // Libera la estación.
        if (station is not null)
        {
            station.Status = StationStatus.Available;
            await _stations.ReplaceAsync(station.Id!, station);
        }

        return ToDto(session);
    }

    public async Task<List<SessionDto>> GetHistoryAsync(string userId)
    {
        var sessions = await _sessions.GetByUserAsync(userId);
        return sessions.Select(ToDto).ToList();
    }

    private static SessionDto ToDto(ChargingSession s) => new(
        s.Id!, s.StationId, s.StationCode, s.StationAddress,
        s.StartTime, s.EndTime, s.EnergyKwh, s.Cost, s.Status);
}
