using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Areas.Identity.Data;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(string? filter)
        {
            var currentUserId = userManager.GetUserId(User);
            
            var query = db.ProjectTasks
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == currentUserId);

            if (filter == "completed")
            {
                query = query.Where(t => t.Status == TaskManager.Models.TaskStatus.Completed);
            }
            else if (filter == "urgent")
            {
                var tomorrow = DateTime.Now.AddDays(1);
                query = query.Where(t => t.EndDate <= tomorrow && t.Status != TaskManager.Models.TaskStatus.Completed);
            }

            var myTasks = await query.OrderByDescending(t => t.StartDate).ToListAsync();

            var allMyTasks = await db.ProjectTasks
                .Where(t => t.AssignedUserId == currentUserId)
                .ToListAsync();

            ViewBag.TotalTasks = allMyTasks.Count;
            ViewBag.ActiveTasks = allMyTasks.Count(t => t.Status == TaskManager.Models.TaskStatus.InProgress);
            ViewBag.CompletedTasks = allMyTasks.Count(t => t.Status == TaskManager.Models.TaskStatus.Completed);
            
            var urgentDeadline = DateTime.Now.AddDays(1);
            ViewBag.UrgentTasks = allMyTasks.Count(t => t.EndDate <= urgentDeadline && t.Status != TaskManager.Models.TaskStatus.Completed);
            
            ViewBag.CurrentFilter = filter;

            return View(myTasks);
        }
    }
}
