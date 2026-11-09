using GaaS_Station.Data;
using GaaS_Station.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using X.PagedList.Extensions;

namespace GaaS_Station.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, IHttpClientFactory clientFactory, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.FirstName.Contains(searchString.Trim()) || s.LastName.Contains(searchString.Trim()) || (s.FirstName + " " + s.LastName).Contains(searchString.Trim()));
            }
            var users = await query.ToListAsync();

            var viewModelList = new List<AdminVM>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                viewModelList.Add(new AdminVM
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    UserRoles = userRoles
                });
            }
            int pageSize = 6;
            int pageNumber = page ?? 1;
            var pagedList = viewModelList.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        [HttpGet]
        public string IndexAJAX(string searchString)
        {
            searchString ??= string.Empty;

            string sql = "SELECT * FROM AspNetUsers WHERE FirstName LIKE @p0 OR LastName LIKE @p0";
            string wrapString = "%" + searchString + "%";

            List<ApplicationUser> users = _context.Users.FromSqlRaw(sql, wrapString).ToList();

            string json = JsonConvert.SerializeObject(users);
            return json;
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var viewModel = new AdminVM
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                AllRoles = allRoles!,
                UserRoles = userRoles
            };

            return View(viewModel);
        }

        // EDIT user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminVM viewModel, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(viewModel.Id);
            if (user == null) return NotFound();

            user.Email = viewModel.Email;
            user.UserName = viewModel.UserName;
            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);

            selectedRoles ??= new List<string>();

            var rolesToAdd = selectedRoles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(selectedRoles);

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return RedirectToAction(nameof(Index));
        }

        // DELETE user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                return RedirectToAction(nameof(Index));

            }
            return NotFound("User not found");
        }

        // Stations
        public async Task<IActionResult> Stations(string searchString, int? page)
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            string apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];

            var client = _clientFactory.CreateClient();
            string url = $"{baseUrl}station/search?apiKey={apiKey}";

            if (!string.IsNullOrEmpty(searchString))
            {
                url += $"&branch={Uri.EscapeDataString(searchString)}";
            }

            var response = await client.GetAsync(url);

            var stationsVMList = new List<StationVM>();

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var apiStations = JsonConvert.DeserializeObject<List<StationVM>>(jsonString) ?? new();

                foreach (var station in apiStations)
                {
                    stationsVMList.Add(new StationVM
                    {
                        StationId = station.StationId,
                        Branch = station.Branch,
                        BrandName = station.BrandName,
                        AddressString = station.AddressString
                    });
                }
            }

            int pageSize = 6;
            int pageNumber = page ?? 1;
            var pagedList = stationsVMList.ToPagedList(pageNumber, pageSize);

            ViewBag.SearchString = searchString;
            return View(pagedList);
        }
        [HttpGet]
        public async Task<IActionResult> StationsAJAX(string searchString)
        {
            searchString ??= string.Empty;

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            string apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];

            var client = _clientFactory.CreateClient();
            string url = $"{baseUrl}station/search?apiKey={apiKey}";

            if (!string.IsNullOrEmpty(searchString))
            {
                url += $"&branch={Uri.EscapeDataString(searchString)}";
            }

            var response = await client.GetAsync(url);
            var stationsVMList = new List<StationVM>();

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var apiStations = JsonConvert.DeserializeObject<List<StationVM>>(jsonString) ?? new();

                foreach (var station in apiStations)
                {
                    stationsVMList.Add(new StationVM
                    {
                        StationId = station.StationId,
                        Branch = station.Branch,
                        BrandName = station.BrandName,
                        AddressString = station.AddressString
                    });
                }
            }

            return Json(stationsVMList);
        }
        public async Task<IActionResult> EditStation(int id)
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            string apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];

            var client = _clientFactory.CreateClient();

            var response = await client.GetAsync($"{baseUrl}station/search?apiKey={apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not connect to station service.";
                return RedirectToAction(nameof(Stations));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var apiStations = JsonConvert.DeserializeObject<List<StationVM>>(jsonString) ?? new();

            var station = apiStations.FirstOrDefault(s => s.StationId == id);

            if (station == null)
            {
                return NotFound();
            }

            var model = new StationVM
            {
                StationId = station.StationId,
                Branch = station.Branch,
                BrandName = station.BrandName,
                AddressString = station.AddressString,
                Latitude = station.Latitude,
                Longitude = station.Longitude
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStation(int id, StationVM model)
        {
            if (id != model.StationId)
            {
                return BadRequest();
            }

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];

            var client = _clientFactory.CreateClient();

            var payload = new
            {
                stationID = model.StationId,
                address = model.AddressString,
                latitude = model.Latitude,
                longitude = model.Longitude,
                brand = model.BrandId,
                branch = model.Branch,
                key = apiKey
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"{baseUrl}station/update", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Station updated successfully!";
                return RedirectToAction(nameof(Stations));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error from API: {errorContent}");
            return View(model);
        }

        public IActionResult CreateStation()
        {
            return View(new StationVM());
        }

        // POST: Admin/CreateStation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStation(StationVM model)
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];

            var client = _clientFactory.CreateClient();

            var payload = new
            {
                address = model.AddressString,
                latitude = model.Latitude,
                longitude = model.Longitude,
                brand = model.BrandId,
                branch = model.Branch,
                key = apiKey
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            // Calls your Azure Function create endpoint
            var response = await client.PostAsync($"{baseUrl}station/create", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Station created successfully!";
                return RedirectToAction(nameof(Stations));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error from API: {errorContent}");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStation(int id)
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];

            var client = _clientFactory.CreateClient();

            var payload = new
            {
                stationID = id,
                key = apiKey
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"{baseUrl}station/delete", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Station deleted successfully!";
                return RedirectToAction(nameof(Stations));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Error deleting station: {errorContent}";
            return RedirectToAction(nameof(Stations));
        }
        public class FuelTypeResponse
        {
            public int FuelID { get; set; }
            public string FuelType { get; set; } = string.Empty;
        }

        private class PriceApiResponse
        {
            public int PriceID { get; set; }
            public int Station { get; set; }
            public string? FuelType { get; set; }
            public decimal PriceValue { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> ManagePrices(int stationId)
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            string apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];
            var client = _clientFactory.CreateClient();

            // 1. Fetch station details
            var stationResponse = await client.GetAsync($"{baseUrl}station/search?apiKey={apiKey}");
            if (!stationResponse.IsSuccessStatusCode) return RedirectToAction(nameof(Stations));

            var stationJson = await stationResponse.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<List<StationVM>>(stationJson) ?? new();
            var station = stations.FirstOrDefault(s => s.StationId == stationId);
            if (station == null) return NotFound();

            // 2. Fetch all fuel types
            var fuelTypes = await GetFuelTypesListAsync();

            // 3. Fetch existing prices for this station
            var priceResponse = await client.GetAsync($"{baseUrl}price/search?key={apiKey}&station={stationId}");
            var existingPrices = new List<StationPriceItemVM>();
            if (priceResponse.IsSuccessStatusCode)
            {
                var priceJson = await priceResponse.Content.ReadAsStringAsync();
                var apiPrices = JsonConvert.DeserializeObject<List<PriceApiResponse>>(priceJson) ?? new();
                existingPrices = apiPrices.Select(p => new StationPriceItemVM
                {
                    PriceId = p.PriceID,
                    StationId = p.Station,
                    FuelTypeId = fuelTypes.FirstOrDefault(f => f.FuelTypeName.Equals(p.FuelType, StringComparison.OrdinalIgnoreCase))?.FuelId ?? 0,
                    PriceValue = p.PriceValue
                }).ToList();
            }

            var model = new StationPriceManagementVM
            {
                StationId = station.StationId,
                StationName = $"{station.BrandName} - {station.Branch}",
                FuelTypes = fuelTypes,
                Prices = existingPrices
            };

            return View(model);
        }

        private Task<List<FuelTypeItemVM>> GetFuelTypesListAsync()
        {
            var fuelTypes = new List<FuelTypeItemVM>
    {
        new FuelTypeItemVM { FuelId = 1, FuelTypeName = "91 (Regular Unleaded)" },
        new FuelTypeItemVM { FuelId = 2, FuelTypeName = "95 (Premium Unleaded)" },
        new FuelTypeItemVM { FuelId = 3, FuelTypeName = "98 (High-Performance Unleaded)" },
        new FuelTypeItemVM { FuelId = 4, FuelTypeName = "Diesel" }
    };

            return Task.FromResult(fuelTypes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStationPrice(int stationId, int fuelTypeId, int priceId, decimal priceValue)
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            string apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];
            var client = _clientFactory.CreateClient();

            HttpResponseMessage response;

            if (priceId > 0)
            {
                var updatePayload = new
                {
                    priceID = priceId,
                    station = stationId,
                    fuelType = fuelTypeId,
                    priceValue = priceValue,
                    key = apiKey
                };
                var content = new StringContent(JsonConvert.SerializeObject(updatePayload), Encoding.UTF8, "application/json");
                response = await client.PostAsync($"{baseUrl}price/update", content);
            }
            else
            {
                var createPayload = new
                {
                    station = stationId,
                    fuelType = fuelTypeId,
                    priceValue = priceValue,
                    key = apiKey
                };
                var content = new StringContent(JsonConvert.SerializeObject(createPayload), Encoding.UTF8, "application/json");
                response = await client.PostAsync($"{baseUrl}price/create", content);
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Fuel price saved successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error saving price: {error}";
            }

            return RedirectToAction(nameof(ManagePrices), new { stationId = stationId });
        }
    }
}
