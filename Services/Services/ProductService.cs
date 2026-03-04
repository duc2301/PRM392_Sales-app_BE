using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;

namespace Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductResponseDTO>>(products);
        }

        public async Task<ProductResponseDTO?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            return product == null ? null : _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.ProductRepository.GetByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductResponseDTO>>(products);
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var products = await _unitOfWork.ProductRepository.GetByPriceRangeAsync(minPrice, maxPrice);
            return _mapper.Map<IEnumerable<ProductResponseDTO>>(products);
        }

        public async Task<IEnumerable<ProductResponseDTO>> SearchAsync(string keyword)
        {
            var products = await _unitOfWork.ProductRepository.SearchAsync(keyword);
            return _mapper.Map<IEnumerable<ProductResponseDTO>>(products);
        }

        public async Task<ProductResponseDTO> CreateAsync(ProductRequestDTO request)
        {
            var product = _mapper.Map<Product>(request);
            await _unitOfWork.ProductRepository.CreateAsync(product);
            await _unitOfWork.SaveChanges();
            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<bool> UpdateAsync(int id, ProductRequestDTO request)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product == null) return false;

            _mapper.Map(request, product);
            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product == null) return false;

            _unitOfWork.ProductRepository.Delete(product);
            await _unitOfWork.SaveChanges();
            return true;
        }
    }
}
