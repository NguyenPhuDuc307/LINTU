using System.ComponentModel.DataAnnotations;

namespace LMS.Data.Entities
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<Message>? Messages { get; set; }
    }
}
