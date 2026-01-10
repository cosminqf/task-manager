using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Areas.Identity.Data;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize] 
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            this.userManager = userManager;
        }

        
        [HttpPost]
        public IActionResult New(Comment comment)
        {
            comment.DateAdded = DateTime.Now;
            
            var userId = userManager.GetUserId(User);
            comment.UserId = userId;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                
                return RedirectToAction("Show", "Tasks", new { id = comment.ProjectTaskId });
            }

            return RedirectToAction("Show", "Tasks", new { id = comment.ProjectTaskId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var comment = db.Comments.Find(id);
            
            if (comment == null)
                return NotFound();
            
            if (comment.UserId != userManager.GetUserId(User))
                return Forbid();
            
            return View(comment);
        }

        [HttpPost]
        public IActionResult Edit(int id, Comment requestComment)
        {
            var comment = db.Comments.Find(id);
            
            if (comment == null)
                return NotFound();
            
            if (comment.UserId != userManager.GetUserId(User))
                return Forbid();
            
            if (ModelState.IsValid)
            {
                comment.Content = requestComment.Content;
                db.SaveChanges();
                
                return RedirectToAction("Show", "Tasks", new { id = comment.ProjectTaskId });
            }
            
            return View(comment);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var comment = db.Comments.Find(id);
            
            if (comment != null && comment.UserId == userManager.GetUserId(User)) 
            {
                var taskId = comment.ProjectTaskId;
                db.Comments.Remove(comment);
                db.SaveChanges();
                return RedirectToAction("Show", "Tasks", new { id = taskId });
            }

            return Forbid();
        }
    }
}
