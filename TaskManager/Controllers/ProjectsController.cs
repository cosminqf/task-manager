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
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _context.Projects
                .Include(p => p.Members)
                .Where(p => p.CreatorId == userId || p.Members.Any(m => m.Id == userId))
                .ToListAsync();
            return View(projects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Creator)
                .Include(p => p.Members)
                .Include(p => p.ProjectTasks) // Asta incarca task-urile
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            return View(project);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            var userId = _userManager.GetUserId(User);
            project.CreatorId = userId;

            ModelState.Remove("Creator");
            ModelState.Remove("CreatorId");

            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (project.CreatorId != userId) return Forbid();

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.Id) return NotFound();

            var userId = _userManager.GetUserId(User);
            var existingProject = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (existingProject == null) return NotFound();
            if (existingProject.CreatorId != userId) return Forbid();

            project.CreatorId = existingProject.CreatorId;
            project.DateCreated = existingProject.DateCreated;

            ModelState.Remove("Creator");
            ModelState.Remove("CreatorId");

            if (ModelState.IsValid)
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (project.CreatorId != userId) return Forbid();

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int projectId, string email)
        {
            var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (project.CreatorId != currentUserId) return Forbid();

            var userToAdd = await _userManager.FindByEmailAsync(email);

            if (userToAdd != null && !project.Members.Contains(userToAdd))
            {
                project.Members.Add(userToAdd);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Membru adaugat cu succes.";
            }
            else
            {
                TempData["Error"] = "Utilizatorul nu exista sau e deja membru.";
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int projectId, string userId)
        {
            var project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (project.CreatorId != currentUserId) return Forbid();

            var userToRemove = project.Members.FirstOrDefault(u => u.Id == userId);
            if (userToRemove != null)
            {
                project.Members.Remove(userToRemove);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Membru sters din proiect.";
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }
    }
}