using APICargadores.DTOs;
using APICargadores.Models;
using APICargadores.Repositories;

namespace APICargadores.Services;

public class StatsService : IStatsService
{
    private readonly IStationRepository _stations;
    private readonly ISessionRepository _sessions;

    public StatsService(IStationRepository stations, ISessionRepository sessions)
    {
        _stations = stations;
        _sessions = sessions;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var total = await _stations.CountAsync();
        var available = await _stations.CountByStatusAsync(StationStatus.Available);
        var inUse = await _stations.CountByStatusAsync(StationStatus.InUse);
        var outOfService = await _stations.CountByStatusAsync(StationStatus.OutOfService);
        var activeSessions = await _sessions.CountByStatusAsync(SessionStatus.Active);
        var (energy, revenue) = await _sessions.GetTotalsAsync();

        return new DashboardDto(total, available, inUse, outOfService,
            activeSessions, Math.Round(energy, 2), revenue);
    }

    public Task<List<SessionsPerDayDto>> GetSessionsPerDayAsync(int days) =>
        _sessions.GetSessionsPerDayAsync(days < 1 ? 7 : days);

    public Task<List<TopStationDto>> GetTopStationsAsync(int limit) =>
        _sessions.GetTopStationsAsync(limit is < 1 or > 50 ? 10 : limit);
}
