using LMS.Data.Entities;

namespace LMS.Models.ViewModels
{
    public class RegisteredClassViewModel
    {
        public ClassRoom ClassRoom { get; set; } = null!;
        public bool IsPaid { get; set; }
    }
}
