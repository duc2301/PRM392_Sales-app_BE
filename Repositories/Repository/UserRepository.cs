using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.DbContexts;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly SalesAppDbContext _context;
        public UserRepository(SalesAppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> Login(string username, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);
        }
    }
}
