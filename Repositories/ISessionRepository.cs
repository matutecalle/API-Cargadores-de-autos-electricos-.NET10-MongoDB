using APICargadores.DTOs;
using APICargadores.Models;

namespace APICargadores.Repositories;

public interface ISessionRepository
{
    Task<ChargingSession> CreateAsync(ChargingSession session);
    Task<ChargingSession?> GetByIdAsync(string id);
    Task<ChargingSession?> GetActiveByStationAsync(string stationId);
    Task<List<ChargingSession>> GetByUserAsync(string userId);
    Task<bool> ReplaceAsync(string id, ChargingSession session);
    Task<long> CountByStatusAsync(SessionStatus status);

    // Aggregation pipelines para el panel de administración.
    Task<(double Energy, decimal Revenue)> GetTotalsAsync();
    Task<List<SessionsPerDayDto>> GetSessionsPerDayAsync(int days);
    Task<List<TopStationDto>> GetTopStationsAsync(int limit);
}
