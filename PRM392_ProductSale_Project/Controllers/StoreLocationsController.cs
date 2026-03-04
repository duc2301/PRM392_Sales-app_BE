using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.Interfaces;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreLocationsController : ControllerBase
    {
        private readonly IStoreLocationService _storeLocationService;

        public StoreLocationsController(IStoreLocationService storeLocationService)
        {
            _storeLocationService = storeLocationService;
        }

        /// <summary>
        /// Get all store locations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                var locations = await _storeLocationService.GetAllLocationsAsync();
                return Ok(ApiResponse.Success("Get store locations success", locations));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get locations failed", ex.Message));
            }
        }

        /// <summary>
        /// Get location by ID
        /// </summary>
        [HttpGet("{locationId}")]
        public async Task<IActionResult> GetLocation(int locationId)
        {
            try
            {
                var location = await _storeLocationService.GetLocationByIdAsync(locationId);
                if (location == null)
                    return NotFound(ApiResponse.Fail("Location not found"));

                return Ok(ApiResponse.Success("Get location success", location));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get location failed", ex.Message));
            }
        }

        /// <summary>
        /// Get locations by city
        /// </summary>
        [HttpGet("city/{city}")]
        public async Task<IActionResult> GetLocationsByCity(string city)
        {
            try
            {
                var locations = await _storeLocationService.GetLocationsByCityAsync(city);
                return Ok(ApiResponse.Success("Get locations by city success", locations));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get locations by city failed", ex.Message));
            }
        }

        /// <summary>
        /// Get nearest store location based on user coordinates
        /// </summary>
        [HttpGet("nearest")]
        public async Task<IActionResult> GetNearestLocation([FromQuery] double latitude, [FromQuery] double longitude)
        {
            try
            {
                var location = await _storeLocationService.GetNearestLocationAsync(latitude, longitude);
                if (location == null)
                    return NotFound(ApiResponse.Fail("No locations found"));

                return Ok(ApiResponse.Success("Get nearest location success", location));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get nearest location failed", ex.Message));
            }
        }
    }
}
