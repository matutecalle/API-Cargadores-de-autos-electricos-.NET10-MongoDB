using APICargadores.DTOs;

namespace APICargadores.Services;

public interface IStatsService
{
    Task<DashboardDto> GetDashboardAsync();
    Task<List<SessionsPerDayDto>> GetSessionsPerDayAsync(int days);
    Task<List<TopStationDto>> GetTopStationsAsync(int limit);
}
