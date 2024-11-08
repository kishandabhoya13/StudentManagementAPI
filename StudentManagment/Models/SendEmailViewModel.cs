namespace StudentManagment.Models
{
    public class SendEmailViewModel
    {
        public List<string> Emails { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string userName { get; set; }

        public string password { get; set; }

        public string JwtToken { get; set; }

        public int RoleId { get; set; }
    }
}
