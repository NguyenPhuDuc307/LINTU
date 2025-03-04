namespace LMS.Data.Entities;

public class ClassDetail : IDateTracking
{
    public Guid ClassRoomId { get; set; }
    public ClassRoom? ClassRoom { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}