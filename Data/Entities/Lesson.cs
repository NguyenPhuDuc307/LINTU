using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities;

public class Lesson : IDateTracking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int Order { get; set; } // Thứ tự hiển thị của lesson

    public int DurationMinutes { get; set; } // Tổng thời lượng của các lecture trong lesson (phút)

    // Foreign key đến ClassRoom
    public Guid ClassRoomId { get; set; }

    // Navigation property
    public ClassRoom? ClassRoom { get; set; }

    // Một Lesson có nhiều Lecture
    public ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();

    // Tracking dates
    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}