using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Controllers
{
    public class DashboardController : Controller
    {
        [Authorize]  
        public IActionResult Index()
        {
            return View();
        }
    }
}
