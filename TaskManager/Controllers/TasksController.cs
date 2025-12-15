using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    public class TasksController(ApplicationDbContext db, IWebHostEnvironment env) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tasks = await db.ProjectTasks.OrderByDescending(t => t.StartDate).ToListAsync();
            return View(tasks);
        }

        [HttpGet]
        public async Task<IActionResult> Show(int id)
        {
            var task = await db.ProjectTasks
                               .Include(t => t.Comments) // <--- INCLUDE COMENTARIILE
                               .ThenInclude(c => c.User) // <--- INCLUDE AUTORUL COMENTARIULUI
                               .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            return View(task);
        }

        [HttpGet]
        public IActionResult New()
        {
            ProjectTask task = new ProjectTask
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
            };
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> New(ProjectTask task)
        {
            if (ModelState.IsValid)
            {
                await HandleMediaUpload(task); 

                db.ProjectTasks.Add(task);
                await db.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Task creat cu succes!";
                return RedirectToAction("Index"); 
            }
            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await db.ProjectTasks.FindAsync(id);
            if (task == null) return NotFound();

            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProjectTask requestTask)
        {
            var existingTask = await db.ProjectTasks.FindAsync(id);
            if (existingTask == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingTask.Title = requestTask.Title;
                existingTask.Description = requestTask.Description;
                existingTask.Status = requestTask.Status;
                existingTask.StartDate = requestTask.StartDate;
                existingTask.EndDate = requestTask.EndDate;
                
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

                db.ProjectTasks.Update(existingTask);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Task actualizat cu succes!";
                return RedirectToAction("Index");
            }

            return View(requestTask);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await db.ProjectTasks.FindAsync(id);
            if (task == null) return NotFound();

            if (!string.IsNullOrEmpty(task.MediaUrl) && task.MediaUrl.StartsWith("/images/"))
            {
                var filePath = Path.Combine(env.WebRootPath, task.MediaUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            db.ProjectTasks.Remove(task);
            await db.SaveChangesAsync();


            TempData["SuccessMessage"] = "Task șters cu succes!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, TaskManager.Models.TaskStatus newStatus)
        {
            var task = await db.ProjectTasks.FindAsync(id);
            if (task == null) return NotFound();

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
            return RedirectToAction("Index");
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