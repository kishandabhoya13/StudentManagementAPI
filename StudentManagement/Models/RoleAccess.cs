namespace StudentManagement_API.Models
{
    public class RoleAccess
    {
        public int RoleAccessId { get; set; }

        public int RoleId { get; set; }

        public bool IsInsert { get; set; } = false;

        public bool IsManaged { get; set; } = false;

        public bool IsViewed { get; set; } = false;
    }
}
