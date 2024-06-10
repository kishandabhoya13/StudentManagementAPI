﻿using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models.DTO
{
    public class JwtClaims
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public int RoleId { get; set; }

        public string? JwtToken { get; set; } = null;
    }
}
