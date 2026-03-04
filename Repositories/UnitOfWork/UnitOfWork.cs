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

        private ICategoryRepository _categoryRepository;
        public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_context);

        private IProductRepository _productRepository;
        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);

        private ICartRepository _cartRepository;
        public ICartRepository CartRepository => _cartRepository ??= new CartRepository(_context);

        private ICartItemRepository _cartItemRepository;
        public ICartItemRepository CartItemRepository => _cartItemRepository ??= new CartItemRepository(_context);

        private INotificationRepository _notificationRepository;
        public INotificationRepository NotificationRepository => _notificationRepository ??= new NotificationRepository(_context);

        private IOrderRepository _orderRepository;
        public IOrderRepository OrderRepository => _orderRepository ??= new OrderRepository(_context);

        private IPaymentRepository _paymentRepository;
        public IPaymentRepository PaymentRepository => _paymentRepository ??= new PaymentRepository(_context);

        private IStoreLocationRepository _storeLocationRepository;
        public IStoreLocationRepository StoreLocationRepository => _storeLocationRepository ??= new StoreLocationRepository(_context);

        private IChatMessageRepository _chatMessageRepository;
        public IChatMessageRepository ChatMessageRepository => _chatMessageRepository ??= new ChatMessageRepository(_context);

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
