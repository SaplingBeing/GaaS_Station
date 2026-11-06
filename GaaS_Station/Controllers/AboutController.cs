using Microsoft.AspNetCore.Mvc;

namespace GaaS_Station.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
