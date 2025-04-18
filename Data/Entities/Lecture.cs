using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities;

public enum LectureContentType
{
    VideoUrl,
    UploadedFile,
    TextContent
}

public class Lecture : IDateTracking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public LectureContentType ContentType { get; set; }

    public string? VideoUrl { get; set; } // URL cho video từ YouTube, Vimeo, etc.

    public string? FileUrl { get; set; } // Đường dẫn đến file đã upload (video, pdf, etc.)

    public string? TextContent { get; set; } // Nội dung text cho lecture dạng HTML

    [Required]
    public int Order { get; set; } // Thứ tự hiển thị trong lesson

    public int DurationMinutes { get; set; } // Thời lượng của lecture (phút)

    // Foreign key đến Lesson
    public int LessonId { get; set; }

    // Navigation property
    public Lesson? Lesson { get; set; }

    // Foreign key đến ClassRoom (redundant nhưng hữu ích để query trực tiếp)
    public Guid ClassRoomId { get; set; }

    // Tracking dates
    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}