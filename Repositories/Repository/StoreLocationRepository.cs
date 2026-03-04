using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.DbContexts;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repository
{
    public class StoreLocationRepository : GenericRepository<StoreLocation>, IStoreLocationRepository
    {
        private readonly SalesAppDbContext _context;

        public StoreLocationRepository(SalesAppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<StoreLocation?> GetByIdAsync(int id)
        {
            return await _context.StoreLocations.FirstOrDefaultAsync(l => l.LocationId == id);
        }

        public async Task<IEnumerable<StoreLocation>> GetByCityAsync(string city)
        {
            // Since StoreLocation model only has Address, we can filter by address containing city
            return await _context.StoreLocations
                .Where(l => l.Address.ToLower().Contains(city.ToLower()))
                .ToListAsync();
        }
    }
}
