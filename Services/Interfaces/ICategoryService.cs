using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;

namespace Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDTO>> GetAllAsync();
        Task<CategoryResponseDTO?> GetByIdAsync(int id);
        Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO request);
        Task<bool> UpdateAsync(int id, CategoryRequestDTO request);
        Task<bool> DeleteAsync(int id);
    }
}
