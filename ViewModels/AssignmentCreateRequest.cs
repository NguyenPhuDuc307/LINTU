public class AssignmentCreateRequest
{
    public String? ClassRoomId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? FileUrl { get; set; }
    public string? DueDate { get; set; }  // Ngày hết hạn dưới dạng chuỗi
}
