
namespace LMS.Data.Entities;

public class Post : IDateTracking
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Guid ClassRoomId { get; set; }
    public ClassRoom? ClassRoom { get; set; }
    public User? User { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}