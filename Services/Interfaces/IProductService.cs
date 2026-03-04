using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;

namespace Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDTO>> GetAllAsync();
        Task<ProductResponseDTO?> GetByIdAsync(int id);
        Task<IEnumerable<ProductResponseDTO>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductResponseDTO>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ProductResponseDTO>> SearchAsync(string keyword);
        Task<ProductResponseDTO> CreateAsync(ProductRequestDTO request);
        Task<bool> UpdateAsync(int id, ProductRequestDTO request);
        Task<bool> DeleteAsync(int id);
    }
}
