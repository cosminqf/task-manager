using System.ComponentModel.DataAnnotations;
using TaskManager.Areas.Identity.Data;

namespace TaskManager.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Conținutul este obligatoriu")]
        public string Content { get; set; } = string.Empty;

        public DateTime DateAdded { get; set; }

        public int ProjectTaskId { get; set; }
        public virtual ProjectTask? ProjectTask { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
