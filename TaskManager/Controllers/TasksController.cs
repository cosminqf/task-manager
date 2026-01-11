using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Areas.Identity.Data;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment env;
        private readonly UserManager<ApplicationUser> userManager;

        public TasksController(ApplicationDbContext db, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.env = env;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tasks = await db.ProjectTasks
                .Include(t => t.AssignedUser)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
            return View(tasks);
        }

        [HttpGet]
        public async Task<IActionResult> Show(int id)
        {
            var task = await db.ProjectTasks
                               .Include(t => t.Comments)
                               .ThenInclude(c => c.User)
                               .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> New(int? projectId)
        {
            if (projectId == null) return NotFound();

            var project = await db.Projects
                .Include(p => p.Creator)
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            
            if (project == null || project.Creator == null) return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var isOrganizer = project.CreatorId == currentUserId;

            // Doar organizatorul poate crea task-uri
            if (!isOrganizer)
            {
                TempData["Error"] = "Doar organizatorul poate crea task-uri noi.";
                return RedirectToAction("Details", "Projects", new { id = projectId });
            }

            // Lista utilizatorilor - organizator + membri proiectului
            var projectUsers = new List<ApplicationUser> { project.Creator };
            projectUsers.AddRange(project.Members);
            
            ViewBag.UsersList = new SelectList(projectUsers, "Id", "Email");

            ProjectTask task = new ProjectTask
            {
                ProjectId = projectId.Value,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
            };
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> New(ProjectTask task)
        {
            var project = await db.Projects
                .Include(p => p.Creator)
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == task.ProjectId);
            
            if (project == null || project.Creator == null) return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var isOrganizer = project.CreatorId == currentUserId;

            // Doar organizatorul poate crea task-uri
            if (!isOrganizer)
            {
                TempData["Error"] = "Doar organizatorul poate crea task-uri noi.";
                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            ModelState.Remove("Project");
            ModelState.Remove("Project.Creator");
            ModelState.Remove("Project.CreatorId");

            if (ModelState.IsValid)
            {
                await HandleMediaUpload(task);

                db.ProjectTasks.Add(task);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Task creat cu succes!";
                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            var projectUsers = new List<ApplicationUser> { project.Creator };
            projectUsers.AddRange(project.Members);
            ViewBag.UsersList = new SelectList(projectUsers, "Id", "Email");
            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await db.ProjectTasks
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Members)
                .Include(t => t.Project!.Creator)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null || task.Project == null) return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var isOrganizer = task.Project.CreatorId == currentUserId;
            var isAssigned = task.AssignedUserId == currentUserId;

            // Doar organizatori sau utilizatori asignați pot edita
            if (!isOrganizer && !isAssigned)
            {
                TempData["Error"] = "Nu aveți permisiunea de a edita acest task.";
                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            // Lista de utilizatori pentru dropdown (doar organizatori văd dropdown-ul)
            if (isOrganizer)
            {
                var projectUsers = new List<ApplicationUser> { task.Project.Creator };
                projectUsers.AddRange(task.Project.Members);
                ViewBag.UsersList = new SelectList(projectUsers, "Id", "Email", task.AssignedUserId);
            }
            
            ViewBag.IsOrganizer = isOrganizer;
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProjectTask requestTask)
        {
            var existingTask = await db.ProjectTasks
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Members)
                .Include(t => t.Project!.Creator)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (existingTask == null || existingTask.Project == null) return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var isOrganizer = existingTask.Project.CreatorId == currentUserId;
            var isAssigned = existingTask.AssignedUserId == currentUserId;

            // Verifică permisiunile
            if (!isOrganizer && !isAssigned)
            {
                TempData["Error"] = "Nu aveți permisiunea de a edita acest task.";
                return RedirectToAction("Details", "Projects", new { id = existingTask.ProjectId });
            }

            ModelState.Remove("Project");

            if (ModelState.IsValid)
            {
                // Organizatorii pot modifica totul
                if (isOrganizer)
                {
                    existingTask.Title = requestTask.Title;
                    existingTask.Description = requestTask.Description;
                    existingTask.Status = requestTask.Status;
                    existingTask.StartDate = requestTask.StartDate;
                    existingTask.EndDate = requestTask.EndDate;
                    existingTask.AssignedUserId = requestTask.AssignedUserId;

                    // Doar organizatorii pot modifica media
                    if (requestTask.TaskImage != null)
                    {
                        if (!string.IsNullOrEmpty(existingTask.MediaUrl) && existingTask.MediaUrl.StartsWith("/images/"))
                        {
                            var oldPath = Path.Combine(env.WebRootPath, existingTask.MediaUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        await HandleMediaUpload(requestTask);
                        existingTask.MediaUrl = requestTask.MediaUrl;
                    }
                    else if (requestTask.MediaUrl != existingTask.MediaUrl)
                    {
                        await HandleMediaUpload(requestTask);
                        existingTask.MediaUrl = requestTask.MediaUrl;
                    }
                }
                // Membrii pot modifica doar statusul
                else
                {
                    existingTask.Status = requestTask.Status;
                }

                db.ProjectTasks.Update(existingTask);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Task actualizat cu succes!";
                return RedirectToAction("Details", "Projects", new { id = existingTask.ProjectId });
            }

            // În caz de eroare, returnează view-ul cu datele necesare
            if (isOrganizer)
            {
                var projectUsers = new List<ApplicationUser> { existingTask.Project.Creator };
                projectUsers.AddRange(existingTask.Project.Members);
                ViewBag.UsersList = new SelectList(projectUsers, "Id", "Email", requestTask.AssignedUserId);
            }
            ViewBag.IsOrganizer = isOrganizer;
            return View(requestTask);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await db.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null || task.Project == null) return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var isOrganizer = task.Project.CreatorId == currentUserId;

            // Doar organizatorul poate șterge task-uri
            if (!isOrganizer)
            {
                TempData["Error"] = "Doar organizatorul poate șterge task-uri.";
                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            if (!string.IsNullOrEmpty(task.MediaUrl) && task.MediaUrl.StartsWith("/images/"))
            {
                var filePath = Path.Combine(env.WebRootPath, task.MediaUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            var projectId = task.ProjectId;

            db.ProjectTasks.Remove(task);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task șters cu succes!";
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, TaskManager.Models.TaskStatus newStatus)
        {
            var task = await db.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null || task.Project == null) return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var isOrganizer = task.Project.CreatorId == currentUserId;
            var isAssigned = task.AssignedUserId == currentUserId;

            // Doar organizatori sau utilizatori asignați pot schimba statusul
            if (!isOrganizer && !isAssigned)
            {
                TempData["Error"] = "Nu aveți permisiunea de a modifica statusul acestui task.";
                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            task.Status = newStatus;
            await db.SaveChangesAsync();

            var statusText = newStatus switch
            {
                TaskManager.Models.TaskStatus.NotStarted => "Neînceput",
                TaskManager.Models.TaskStatus.InProgress => "În Progres",
                TaskManager.Models.TaskStatus.Completed => "Finalizat",
                _ => newStatus.ToString()
            };

            TempData["SuccessMessage"] = $"Status schimbat în '{statusText}' cu succes!";
            return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
        }

        private async Task HandleMediaUpload(ProjectTask task)
        {
            if (task.TaskImage != null && task.TaskImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(task.TaskImage.FileName);
                var storagePath = Path.Combine(env.WebRootPath, "images", fileName);

                var folderPath = Path.Combine(env.WebRootPath, "images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await task.TaskImage.CopyToAsync(fileStream);
                }
                task.MediaUrl = "/images/" + fileName;
            }
            else if (!string.IsNullOrEmpty(task.MediaUrl) && task.MediaUrl.Contains("youtube.com") && task.MediaUrl.Contains("watch?v="))
            {
                var videoId = task.MediaUrl.Split("v=")[1].Split('&')[0];
                task.MediaUrl = $"https://www.youtube.com/embed/{videoId}";
            }
        }
    }
}