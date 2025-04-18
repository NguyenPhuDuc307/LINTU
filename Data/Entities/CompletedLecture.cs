using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities;

public class CompletedLecture
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

    // Ngày hoàn thành
    public DateTime CompletedDate { get; set; }
}