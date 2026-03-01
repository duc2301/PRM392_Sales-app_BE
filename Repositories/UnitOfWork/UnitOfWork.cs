using Repositories.DbContexts;
using Repositories.Interfaces;
using Repositories.Repository;

namespace Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SalesAppDbContext _context;

        public UnitOfWork(SalesAppDbContext context)
        {
            _context = context;
        }

        private IUserRepository _userRepository;
        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
