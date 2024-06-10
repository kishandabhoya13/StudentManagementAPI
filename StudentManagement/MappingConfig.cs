using AutoMapper;
using DemoAPI.Models.DTO;
using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;

namespace StudentManagment_API
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            CreateMap<JwtClaims, Student>().ReverseMap();
            CreateMap<JwtClaims, ProfessorHod>().ReverseMap();

        }
    }
}
