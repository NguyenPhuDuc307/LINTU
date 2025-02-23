namespace LMS.Data.Entities;

public class Topic{
    public int Id { get; set;}
    public string? Name { get; set;}
    public string? Alias { get; set;}
    public string? ImageUrl { get; set;}
    public int ParentTopicId { get; set;}
    public ICollection<ClassRoom>? ClassRooms { get; set;}
}