using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request);
        Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO request);
    }
}
