using Microsoft.AspNetCore.Mvc;

namespace GaaS_Station.Controllers
{
    public class GaasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
