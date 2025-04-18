using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities;

public class LectureNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // Foreign key đến Lecture
    public int LectureId { get; set; }
    public Lecture? Lecture { get; set; }

    // Foreign key đến User
    public string? UserId { get; set; }
    public User? User { get; set; }

    // Foreign key đến ClassRoom (redundant nhưng hữu ích để query)
    public Guid ClassRoomId { get; set; }
    public ClassRoom? ClassRoom { get; set; }

    // Nội dung ghi chú
    public string? Notes { get; set; }

    // Ngày cập nhật
    public DateTime UpdatedDate { get; set; }
}