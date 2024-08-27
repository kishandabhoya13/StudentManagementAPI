namespace StudentManagment.Models
{
    public class StudentsCountFromDateViewModel
    {
        public DateTime? CreatedDate { get; set; } = null;

        public int StudentsCount { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

    }
}
