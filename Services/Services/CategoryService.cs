using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;

namespace Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryResponseDTO>>(categories);
        }

        public async Task<CategoryResponseDTO?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            return category == null ? null : _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO request)
        {
            var category = _mapper.Map<Category>(request);
            await _unitOfWork.CategoryRepository.CreateAsync(category);
            await _unitOfWork.SaveChanges();
            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<bool> UpdateAsync(int id, CategoryRequestDTO request)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            _mapper.Map(request, category);
            _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            _unitOfWork.CategoryRepository.Delete(category);
            await _unitOfWork.SaveChanges();
            return true;
        }
    }
}
