using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Interfaces;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Lấy tất cả danh mục sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return Ok(ApiResponse.Success("Get categories success", categories));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get categories failed", ex.Message));
            }
        }

        /// <summary>
        /// Lấy chi tiết danh mục theo ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResponse.Fail("Category not found"));

                return Ok(ApiResponse.Success("Get category success", category));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get category failed", ex.Message));
            }
        }

        /// <summary>
        /// Tạo danh mục mới (Admin)
        /// </summary>
        /// <param name="request">Category data</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var category = await _categoryService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, 
                    ApiResponse.Success("Create category success", category));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Create category failed", ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật danh mục (Admin)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="request">Updated category data</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var result = await _categoryService.UpdateAsync(id, request);
                if (!result)
                    return NotFound(ApiResponse.Fail("Category not found"));

                return Ok(ApiResponse.Success("Update category success"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Update category failed", ex.Message));
            }
        }

        /// <summary>
        /// Xóa danh mục (Admin)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _categoryService.DeleteAsync(id);
                if (!result)
                    return NotFound(ApiResponse.Fail("Category not found"));

                return Ok(ApiResponse.Success("Delete category success"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Delete category failed", ex.Message));
            }
        }
    }
}
