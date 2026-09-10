using APICargadores.DTOs;
using APICargadores.Models;

namespace APICargadores.Repositories;

public interface IStationRepository
{
    Task<(List<Station> Items, long Total)> FilterAsync(StationFilterDto filter);
    Task<List<Station>> FindNearbyAsync(double latitude, double longitude, double maxKm, int limit);
    Task<Station?> GetByIdAsync(string id);
    Task<Station?> GetByStationCodeAsync(string stationCode);
    Task<Station> CreateAsync(Station station);
    Task<bool> ReplaceAsync(string id, Station station);
    Task<bool> DeleteAsync(string id);
    Task<long> CountAsync();
    Task<long> CountByStatusAsync(StationStatus status);
}
