using Microsoft.AspNetCore.Identity;
using TaskManager.Models;

namespace TaskManager.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}