﻿namespace StudentManagement_API.Models.Models.DTO
{
    public class SendEmailDto
    {
        public int StudentId { get; set; }
        public List<string> Emails { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
