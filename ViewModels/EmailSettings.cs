using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.ViewModels
{
    public class EmailSettings
    {
        public string? FromName { get; set; }
        public string? From { get; set; }
        public string? Password { set; get; }
    }
}