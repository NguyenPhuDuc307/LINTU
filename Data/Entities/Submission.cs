using LMS.Data.Entities;
namespace LMS.Data.Entities;
public class Submission
{
    public int Id { get; set; }
    public string? UserId { get; set; } // Người nộp bài
    public User? User { get; set; }
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }
    public string? AnswerText { get; set; } // Nội dung trả lời
    public string? FileUrl { get; set; } // Đường dẫn file nộp
    public DateTime SubmitDate { get; set; }
}
