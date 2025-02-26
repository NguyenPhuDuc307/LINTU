using Microsoft.AspNetCore.Identity;

namespace LMS.Data.Entities;

public class User : IdentityUser
{
    public string? FullName{ get; set; }
    public DateTime DateOfBirth { get; set; }
    public ICollection<Post>? Posts { get; set; }
    public ICollection<ClassDetail>? ClassDetails { get; set; }
}