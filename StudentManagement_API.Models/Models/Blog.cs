using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models
{
    public class Blog
    {
        public int BlogId { get; set; }

        [Required(ErrorMessage = "Enter Short Description")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Enter Long Description")]
        public string LongDescription { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; }

        public bool IsDeleted { get; set; } = false;

        public string? ImageName { get; set; } = null;

        public int? RowNumber { get; set; } = 1;

        public int? TotalRecords { get; set; } = 0;
    }
}
