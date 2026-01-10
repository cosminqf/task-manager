using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.Areas.Identity.Data;

namespace TaskManager.Models;

public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed
}

public class ProjectTask : IValidatableObject
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul este necesar")]
    [StringLength(200, ErrorMessage = "Titlul nu poate depăși 200 de caractere")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrierea este necesară")]
    [StringLength(2000, ErrorMessage = "Descrierea nu poate depăși 2000 de caractere")]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

    [Required(ErrorMessage = "Data de început este necesară")]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Data de final este necesară")]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }
    
    [NotMapped]
    [Display(Name = "Image Task")]
    public IFormFile? TaskImage { get; set; }

    [Display(Name = "Media URL")]
    [StringLength(500, ErrorMessage = "URL-ul media nu poate depăși 500 de caractere")]
    public string? MediaUrl { get; set; }


    [Display(Name = "Postat de")]
    public string? AssignedUserId { get; set; }

    [ForeignKey("AssignedUserId")]
    public virtual ApplicationUser? AssignedUser { get; set; }

    [Display(Name = "Proiect")]
    public int? ProjectId { get; set; }

    // [ForeignKey("ProjectId")]
    // public virtual Project? Project { get; set; } 


    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate < StartDate)
        {
            yield return new ValidationResult(
                "Data de final trebuie să fie mai mare decât data de început.",
                new[] { nameof(EndDate) });
        }
    }
}

