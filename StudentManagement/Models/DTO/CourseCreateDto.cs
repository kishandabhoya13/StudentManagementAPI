﻿using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models.DTO
{
    public class CourseCreateDto
    {
        [Required,StringLength(50)]
        public string Name { get; set; }

    }
}
