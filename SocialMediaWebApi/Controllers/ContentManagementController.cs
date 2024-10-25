using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class ContentManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
