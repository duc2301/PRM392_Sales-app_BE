using AutoMapper;
using Repositories.Models;
using Services.DTOs.ResponseDTOs;

namespace Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDTO>().ReverseMap();
        }
    }
}
