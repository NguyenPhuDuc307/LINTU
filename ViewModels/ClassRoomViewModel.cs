using LMS.Data.Entities;

namespace LMS.ViewModels;

public class ClassRoomViewModel
{
    public ClassRoom? ClassRoom { get; set; }
    public IEnumerable<Post>? Posts { get; set; }
    public int MembersCount { get; set; }
}