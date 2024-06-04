﻿using StudentManagment.Models.DataModels;
using System.Net;

namespace StudentManagement.Models
{
    public class APIResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess { get; set; } = true;
        
        public List<string> ErroMessages { get; set; }

        public T result { get; set; }
    }
}