using Services.DTOs.ResponseDTOs;

namespace Services.Interfaces
{
    public interface IStoreLocationService
    {
        Task<IEnumerable<StoreLocationDTO>> GetAllLocationsAsync();
        Task<StoreLocationDTO?> GetLocationByIdAsync(int locationId);
        Task<IEnumerable<StoreLocationDTO>> GetLocationsByCityAsync(string city);
        Task<StoreLocationDTO?> GetNearestLocationAsync(double userLatitude, double userLongitude);
    }
}
