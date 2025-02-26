using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities;

public class ClassRoom : IDateTracking
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int TopicId { get; set; }
    public Topic? Topic { get; set; }
    public string? Introduction { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Code { get; set; }
    [Column(TypeName = "decimal(18,0)")]
    public double Price { get; set; }
    public int Students { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public ICollection<Post>? Posts { get; set; }
    public ICollection<ClassDetail>? ClassDetails { get; set; }
}