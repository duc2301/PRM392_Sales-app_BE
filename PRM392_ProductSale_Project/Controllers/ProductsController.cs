using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Interfaces;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Lấy tất cả sản phẩm với phân trang, sắp xếp, lọc
        /// </summary>
        /// <param name="categoryId">Filter by category ID (optional)</param>
        /// <param name="minPrice">Filter by minimum price (optional)</param>
        /// <param name="maxPrice">Filter by maximum price (optional)</param>
        /// <param name="search">Search by name or description (optional)</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string search = null)
        {
            try
            {
                IEnumerable<dynamic> products;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    products = await _productService.SearchAsync(search);
                }
                else if (categoryId.HasValue)
                {
                    products = await _productService.GetByCategoryAsync(categoryId.Value);
                }
                else if (minPrice.HasValue && maxPrice.HasValue)
                {
                    products = await _productService.GetByPriceRangeAsync(minPrice.Value, maxPrice.Value);
                }
                else
                {
                    products = await _productService.GetAllAsync();
                }

                return Ok(ApiResponse.Success("Get products success", products));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get products failed", ex.Message));
            }
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm theo ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(ApiResponse.Fail("Product not found"));

                return Ok(ApiResponse.Success("Get product success", product));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get product failed", ex.Message));
            }
        }

        /// <summary>
        /// Lấy sản phẩm theo danh mục
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns></returns>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetByCategoryAsync(categoryId);
                return Ok(ApiResponse.Success("Get products by category success", products));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get products by category failed", ex.Message));
            }
        }

        /// <summary>
        /// Tạo sản phẩm mới (Admin)
        /// </summary>
        /// <param name="request">Product data</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var product = await _productService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, 
                    ApiResponse.Success("Create product success", product));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Create product failed", ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm (Admin)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="request">Updated product data</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var result = await _productService.UpdateAsync(id, request);
                if (!result)
                    return NotFound(ApiResponse.Fail("Product not found"));

                return Ok(ApiResponse.Success("Update product success"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Update product failed", ex.Message));
            }
        }

        /// <summary>
        /// Xóa sản phẩm (Admin)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _productService.DeleteAsync(id);
                if (!result)
                    return NotFound(ApiResponse.Fail("Product not found"));

                return Ok(ApiResponse.Success("Delete product success"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Delete product failed", ex.Message));
            }
        }
    }
}
