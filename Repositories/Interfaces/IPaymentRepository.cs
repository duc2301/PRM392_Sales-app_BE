using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
    }
}
