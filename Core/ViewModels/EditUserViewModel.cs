
using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMS.Core.ViewModels
{
    public class EditUserViewModel
    {
        public User? User { get; set; }

        public IList<SelectListItem>? Roles { get; set; }
    }
}
