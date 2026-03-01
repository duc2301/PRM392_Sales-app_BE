using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid? id);
        Task CreateAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        void DeleteById(Guid id);
        Task<T?> GetByIdAsync(int? id);
        void DeleteById(int id);
    }
}
