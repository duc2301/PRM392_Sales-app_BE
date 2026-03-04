using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Interfaces;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// User Registration - Create new account
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var result = await _authService.RegisterAsync(request);
                return Created(nameof(Register), 
                    ApiResponse.Success("Registration successful", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Registration failed", ex.Message));
            }
        }

        /// <summary>
        /// User Login - Authenticate user
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var result = await _authService.LoginAsync(request);
                if (result == null)
                    return Unauthorized(ApiResponse.Fail("Invalid username or password"));

                return Ok(ApiResponse.Success("Login successful", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Login failed", ex.Message));
            }
        }
    }
}
