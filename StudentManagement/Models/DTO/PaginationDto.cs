namespace StudentManagement_API.Models.DTO
{
    public class PaginationDto
    {
        //public int CurrentPageNumber { get; set; } = 0;

        public int StartIndex { get; set; } = 0;

        public int PageSize { get; set; } = 10;

        public string? searchQuery { get; set; } = null;

        public string? OrderBy { get; set; } = null;

        public string? OrderDirection { get; set; } = null;

        public string? JwtToken { get; set; } = null;
    }
}
