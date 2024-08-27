namespace StudentManagement_API.Models.Models.DTO
{
    public class PaginationDto
    {
        //public int CurrentPageNumber { get; set; } = 0;

        public int StartIndex { get; set; } = 0;

        public int PageSize { get; set; } = 5;

        public string? searchQuery { get; set; } = null;

        public string? OrderBy { get; set; } = null;

        public string? OrderDirection { get; set; } = null;

        public string? JwtToken { get; set; } = null;

        public DateOnly? FromDate { get; set; } = null;

        public DateOnly? ToDate { get; set; } = null;



    }
}
