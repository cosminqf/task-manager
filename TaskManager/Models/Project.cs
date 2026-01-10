using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.Areas.Identity.Data;

namespace TaskManager.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        [Required]
        public string CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public virtual ApplicationUser Creator { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public virtual ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
    }
}