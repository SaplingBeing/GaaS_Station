using GaaS_Station.Data;
using GaaS_Station.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GaaS_Station.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return View();

            try
            {
                var favourites = await _context.UserFavourites.Where(f => f.UserId == userId).Select(f => f.StationId).ToListAsync();
                if (favourites.Count == 0) return View();

                var apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"] ?? "";
                var geoResponse = await _httpClientFactory.CreateClient("GeoLocationApi").GetAsync($"api/location/query?longitude=174.76&latitude=-36.85&radiusKm=100&apiKey={Uri.EscapeDataString(apiKey)}");
                if (!geoResponse.IsSuccessStatusCode) return View();

                var results = (await geoResponse.Content.ReadFromJsonAsync<List<StationResult>>() ?? new List<StationResult>()).Where(r => favourites.Contains(r.StationID)).ToList();

                var priceResponse = await _httpClientFactory.CreateClient("GaaSApi").GetAsync($"price/search?apiKey={Uri.EscapeDataString(apiKey)}&Code={_configuration["ApiSettings:Key"]}");
                var prices = priceResponse.IsSuccessStatusCode ? await priceResponse.Content.ReadFromJsonAsync<List<PriceResult>>() : new List<PriceResult>();

                foreach (var result in results)
                {
                    result.Icon = (result.Brand?.ToLower()) switch
                    {
                        "z energy" => "/images/z.png",
                        "bp" => "/images/bp.png",
                        "caltex" => "/images/caltex.png",
                        _ => null
                    };
                    result.Prices = prices.Where(p => p.StationName == $"{result.Brand} {result.Branch}").ToList();
                }

                ViewData["Favourites"] = results;
            }
            catch { }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsConditions()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string locationTb, string station = "", string fuelType = "")
        {
            // Send everything to gaas page so the map and sidebar do the search there
            return RedirectToAction("Index", "Gaas", new { locationTb, station, fuelType });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
