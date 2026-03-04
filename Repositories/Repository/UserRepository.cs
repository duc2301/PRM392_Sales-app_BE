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

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }
    }
}

