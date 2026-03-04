using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;
using Services.Utils;

namespace Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request)
        {
            // Check if user exists
            var existingUser = await _unitOfWork.UserRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                throw new Exception("Username already exists");

            // Check if email exists
            var existingEmail = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
                throw new Exception("Email already exists");

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Role = "user",
                PasswordHash = PasswordHasher.HashPassword(request.Password)
            };

            await _unitOfWork.UserRepository.CreateAsync(user);
            await _unitOfWork.SaveChanges();

            // Generate token with all user info
            var token = _tokenService.GenerateToken(
                user.UserId, 
                user.Username, 
                user.Email, 
                user.Role, 
                user.PhoneNumber, 
                user.Address
            );

            return new AuthResponseDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role,
                Token = token
            };
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO request)
        {
            // Find user by username
            var user = await _unitOfWork.UserRepository.GetByUsernameAsync(request.Username);
            if (user == null)
                return null;

            // Verify password
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            // Generate token with all user info
            var token = _tokenService.GenerateToken(
                user.UserId, 
                user.Username, 
                user.Email, 
                user.Role, 
                user.PhoneNumber, 
                user.Address
            );

            return new AuthResponseDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role,
                Token = token
            };
        }
    }
}
