using AutoMapper;
using Repositories.Interfaces;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllUser()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDTO>>(users);
        }

        public async Task<string> Login(string username, string password)
        {
            var user = await _userRepository.Login(username, password);
            if (user == null) {
                return null;
            }
            return "Login thanh cong";
        }
    }
}
