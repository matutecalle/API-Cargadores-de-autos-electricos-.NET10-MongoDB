using APICargadores.DTOs;

namespace APICargadores.Services;

public interface ISessionService
{
    Task<SessionDto> StartAsync(string userId, StartSessionDto dto);
    Task<SessionDto> StopAsync(string userId, string sessionId, StopSessionDto dto);
    Task<List<SessionDto>> GetHistoryAsync(string userId);
}
