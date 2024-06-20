using AutoMapper;
using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;

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
