using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("ChatRoom")]
        public int ChatRoomId { get; set; }
        public ChatRoom? ChatRoom { get; set; }
    }
}
