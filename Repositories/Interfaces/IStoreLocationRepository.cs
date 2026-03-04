using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IStoreLocationRepository : IGenericRepository<StoreLocation>
    {
        Task<StoreLocation?> GetByIdAsync(int id);
        Task<IEnumerable<StoreLocation>> GetByCityAsync(string city);
    }
}
