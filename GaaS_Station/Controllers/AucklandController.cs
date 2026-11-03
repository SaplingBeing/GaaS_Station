using Microsoft.AspNetCore.Mvc;

namespace GaaS_Station.Controllers
{
    public class AucklandController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
