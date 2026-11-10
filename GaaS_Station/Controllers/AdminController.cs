using GaaS_Station.Data;
using GaaS_Station.Models;
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
        private readonly string _baseUrl;
        private readonly string _apiKey;

        // Constructor
        public AdminController(ApplicationDbContext context, IHttpClientFactory clientFactory, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            // Gets baseUrl and Admin api key from appsettings.json
            _baseUrl = _configuration["ApiSettings:BaseUrl"];
            _apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"];
        }

        // Display all users | Admin/Index.cshtml
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var query = _userManager.Users.AsQueryable();

            // A query to find a user with a specific first or last name 
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.FirstName.Contains(searchString.Trim()) || s.LastName.Contains(searchString.Trim()) || (s.FirstName + " " + s.LastName).Contains(searchString.Trim()));
            }

            // Executes the query 
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

            // Displays 6 rows per page 
            int pageSize = 6;
            int pageNumber = page ?? 1;
            var pagedList = viewModelList.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        // Search suggestion for Admin/Index.cshtml
        [HttpGet]
        public string IndexAJAX(string searchString)
        {   // Ensure search string is not null
            searchString ??= string.Empty;

            // SQL query for first and last name
            string sql = "SELECT * FROM AspNetUsers WHERE FirstName LIKE @p0 OR LastName LIKE @p0";
            string wrapString = "%" + searchString + "%";

            List<ApplicationUser> users = _context.Users.FromSqlRaw(sql, wrapString).ToList();
            string json = JsonConvert.SerializeObject(users);
            return json;
        }
        // Displays the edit view for the specific user id | Admin/Edit/id 
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

        // Updates user details and roles 
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

            // Role synchronisation 
            // Fetch user's current and [user] selected roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            selectedRoles ??= new List<string>();

            // Checks which roles needs to be added and removed
            var rolesToAdd = selectedRoles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(selectedRoles);

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return RedirectToAction(nameof(Index));
        }

        // Deletes user if found
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

        // Displays all stations | Stations.cshtml
        public async Task<IActionResult> Stations(string searchString, int? page)
        {
            var client = _clientFactory.CreateClient();
            string url = $"{_baseUrl}station/search?apiKey={_apiKey}";

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

        // Search suggestion for Admin/Stations
        [HttpGet]
        public async Task<IActionResult> StationsAJAX(string searchString)
        {
            searchString ??= string.Empty;

            var client = _clientFactory.CreateClient();
            string url = $"{_baseUrl}station/search?apiKey={_apiKey}";

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

        // Displays edit view for the sepcific station, using station id | Admin/EditStation/Id
        public async Task<IActionResult> EditStation(int id)
        {
            var client = _clientFactory.CreateClient();

            var response = await client.GetAsync($"{_baseUrl}station/search?apiKey={_apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Stations));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var apiStations = JsonConvert.DeserializeObject<List<StationVM>>(jsonString) ?? new();

            var station = apiStations.FirstOrDefault(s => s.StationId == id);

            if (station == null)
            {
                return NotFound();
            }

            var stationVM = new StationVM
            {
                StationId = station.StationId,
                Branch = station.Branch,
                BrandName = station.BrandName,
                AddressString = station.AddressString,
                Latitude = station.Latitude,
                Longitude = station.Longitude
            };

            return View(stationVM);
        }

        // Updates station details 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStation(int id, StationVM stationVM)
        {
            if (id != stationVM.StationId)
            {
                return BadRequest();
            }

            var client = _clientFactory.CreateClient();

            var payload = new
            {
                stationID = stationVM.StationId,
                address = stationVM.AddressString,
                latitude = stationVM.Latitude,
                longitude = stationVM.Longitude,
                brand = stationVM.BrandId,
                branch = stationVM.Branch,
                key = _apiKey
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8,"application/json");

            var response = await client.PostAsync($"{_baseUrl}station/update", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Stations));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error from API: {errorContent}");
            return View(stationVM);
        }

        public IActionResult CreateStation()
        {
            return View(new StationVM());
        }

        // Create a new station
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStation(StationVM stationVM)
        {
            var client = _clientFactory.CreateClient();

            var payload = new
            {
                address = stationVM.AddressString,
                latitude = stationVM.Latitude,
                longitude = stationVM.Longitude,
                brand = stationVM.BrandId,
                branch = stationVM.Branch,
                key = _apiKey
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_baseUrl}station/create", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Stations));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error creating station: {error}";
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error from API: {errorContent}");
            return View(stationVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStation(int id)
        {
            var client = _clientFactory.CreateClient();

            var payload = new
            {
                stationID = id,
                key = _apiKey
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_baseUrl}station/delete", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Stations));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Error deleting station: {errorContent}";
            return RedirectToAction(nameof(Stations));
        }

        // Defines FuelType and corresponding id 
        private Task<List<FuelType>> GetFuelTypesListAsync()
        {
            // Hardcoded fuel types to reflect dbo.Fuel type
            var fuelTypes = new List<FuelType>
            {
                new FuelType { FuelID = 1, FuelTypeName = "91 (Regular Unleaded)" },
                new FuelType { FuelID = 2, FuelTypeName = "95 (Premium Unleaded)" },
                new FuelType { FuelID = 3, FuelTypeName = "98 (High-Performance Unleaded)" },
                new FuelType { FuelID = 4, FuelTypeName = "Diesel" }
            };
            return Task.FromResult(fuelTypes);
        }

        [HttpGet]
        public async Task<IActionResult> ManagePrices(int stationId)
        {
            var client = _clientFactory.CreateClient();

            // Fetch station details
            var stationResponse = await client.GetAsync($"{_baseUrl}station/search?apiKey={_apiKey}");
            if (!stationResponse.IsSuccessStatusCode) return RedirectToAction(nameof(Stations));

            var stationJson = await stationResponse.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<List<StationVM>>(stationJson) ?? new();
            var station = stations.FirstOrDefault(s => s.StationId == stationId);
            if (station == null) return NotFound();

            // Fetch all fuel types
            var fuelTypes = await GetFuelTypesListAsync();

            // Fetch existing prices for this station
            var priceResponse = await client.GetAsync($"{_baseUrl}price/search?key={_apiKey}&station={stationId}");
            var existingPrices = new List<Price>();
            if (priceResponse.IsSuccessStatusCode)
            {
                var priceJson = await priceResponse.Content.ReadAsStringAsync();
                var apiPrices = JsonConvert.DeserializeObject<List<Price>>(priceJson) ?? new();
                existingPrices = apiPrices.Select(p => new Price
                {
                    PriceID = p.PriceID,
                    Station = p.Station,
                    FuelTypeId = fuelTypes.FirstOrDefault(f => f.FuelTypeName.Equals(p.FuelType, StringComparison.OrdinalIgnoreCase))?.FuelID ?? 0,
                    PriceValue = p.PriceValue
                }).ToList();
            }

            var stationPriceVM = new StationPriceVM
            {
                StationId = station.StationId,
                StationName = $"{station.BrandName} - {station.Branch}",
                FuelTypes = fuelTypes,
                Prices = existingPrices
            };

            return View(stationPriceVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStationPrice(int stationId, int fuelTypeId, int priceId, decimal priceValue)
        {
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
                    key = _apiKey
                };
                var content = new StringContent(JsonConvert.SerializeObject(updatePayload), Encoding.UTF8, "application/json");
                response = await client.PostAsync($"{_baseUrl}price/update", content);
            }
            else
            {
                var createPayload = new
                {
                    station = stationId,
                    fuelType = fuelTypeId,
                    priceValue = priceValue,
                    key = _apiKey
                };
                var content = new StringContent(JsonConvert.SerializeObject(createPayload), Encoding.UTF8, "application/json");
                response = await client.PostAsync($"{_baseUrl}price/create", content);
            }
            // Message if price sucessfully updates or not
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