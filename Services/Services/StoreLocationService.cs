using AutoMapper;
using Repositories.Interfaces;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;

namespace Services.Services
{
    public class StoreLocationService : IStoreLocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoreLocationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StoreLocationDTO>> GetAllLocationsAsync()
        {
            var locations = await _unitOfWork.StoreLocationRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<StoreLocationDTO>>(locations);
        }

        public async Task<StoreLocationDTO?> GetLocationByIdAsync(int locationId)
        {
            var location = await _unitOfWork.StoreLocationRepository.GetByIdAsync(locationId);
            return location == null ? null : _mapper.Map<StoreLocationDTO>(location);
        }

        public async Task<IEnumerable<StoreLocationDTO>> GetLocationsByCityAsync(string city)
        {
            var locations = await _unitOfWork.StoreLocationRepository.GetByCityAsync(city);
            return _mapper.Map<IEnumerable<StoreLocationDTO>>(locations);
        }

        public async Task<StoreLocationDTO?> GetNearestLocationAsync(double userLatitude, double userLongitude)
        {
            var locations = await _unitOfWork.StoreLocationRepository.GetAllAsync();

            if (!locations.Any())
                return null;

            var nearestLocation = locations.OrderBy(l =>
                CalculateDistance(userLatitude, userLongitude, (double)l.Latitude, (double)l.Longitude)
            ).FirstOrDefault();

            return nearestLocation == null ? null : _mapper.Map<StoreLocationDTO>(nearestLocation);
        }

        /// <summary>
        /// Calculate distance between two coordinates using Haversine formula (km)
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in kilometers

            var dLat = DegreeToRadian(lat2 - lat1);
            var dLon = DegreeToRadian(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreeToRadian(lat1)) * Math.Cos(DegreeToRadian(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double DegreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }
    }
}
