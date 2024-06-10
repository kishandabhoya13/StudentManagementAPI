﻿using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models
{
    public class AdminViewModel
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public int RoleId { get; set; }

        public string? JwtToken { get; set; } = null;
    }
}
