using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels;

public class LessonCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài học")]
    [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thứ tự bài học")]
    [Range(1, 1000, ErrorMessage = "Thứ tự phải từ 1 đến 1000")]
    public int Order { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn lớp học")]
    public Guid ClassRoomId { get; set; }
}