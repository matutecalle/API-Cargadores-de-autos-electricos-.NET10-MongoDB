using APICargadores.DTOs;

namespace APICargadores.Services;

public interface IStationService
{
    Task<(List<StationDto> Items, long Total)> SearchAsync(StationFilterDto filter);
    Task<List<StationDto>> NearbyAsync(double lat, double lng, double maxKm, int limit);
    Task<StationDto?> GetByIdAsync(string id);
    Task<StationDto> CreateAsync(CreateStationDto dto);
    Task<StationDto?> UpdateAsync(string id, UpdateStationDto dto);
    Task<bool> DeleteAsync(string id);
}
