using Microsoft.AspNetCore.Mvc;

namespace GaaS_Station.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
