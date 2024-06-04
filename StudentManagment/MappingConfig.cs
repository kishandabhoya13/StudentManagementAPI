﻿using AutoMapper;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;

namespace StudentManagment
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            CreateMap<StudentViewModel, Student>().ReverseMap();
        }
    }
}
