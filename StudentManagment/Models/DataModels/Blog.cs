using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models.DataModels
{
    public class Blog
    {
        public int BlogId { get; set; }

        [Required(ErrorMessage = "Enter Short Description")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Enter Long Description")]
        public string LongDescription { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set;} = DateTime.Now;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; }

        public bool IsDeleted { get; set; } = false;

        public IFormFile? Image { get; set; }

        public string? ImageName { get; set; } = null;

        public IList<Blog> Blogs { get; set; }

        public string? Language { get; set; } = null;

    }

    public class GoogleTranslateResponse
    {
        public TranslationData Data { get; set; }
    }

    public class TranslationData
    {
        public Translation[] Translations { get; set; }
    }

    public class Translation
    {
        public string TranslatedText { get; set; }
    }
}
