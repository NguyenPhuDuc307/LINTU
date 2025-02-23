namespace LMS.ViewModels;

public class PostCreateRequest
{
    public int ClassRoomId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
}