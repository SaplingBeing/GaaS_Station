using GaaS_Station.Models;
using Microsoft.AspNetCore.Mvc;

namespace GaaS_Station.Controllers
{
    public class GaasController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _azKey;

        public GaasController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _azKey = _configuration["ApiSettings:Key"];
        }

        public async Task<IActionResult> Index(string? locationTb, string? station, string? fuelType, decimal? latitude, decimal? longitude)
        {
            ViewData["SearchLocation"] = locationTb;
            ViewData["SearchStation"] = station;
            ViewData["SearchFuelType"] = fuelType;
            ViewData["Latitude"] = latitude;
            ViewData["Longitude"] = longitude;

            bool hasLocation = !string.IsNullOrWhiteSpace(locationTb);
            bool hasStation = !string.IsNullOrEmpty(station) && station != "Station";
            bool hasFuelType = !string.IsNullOrEmpty(fuelType) && fuelType != "Fuel Type";
            bool searched = hasLocation || hasStation || hasFuelType;
            ViewData["Searched"] = searched;

            // No search and no location yet, the page will ask the browser for it
            if (!searched && (!latitude.HasValue || !longitude.HasValue)) return View();

            try
            {
                var apiKey = _configuration["ApiSettings:GAAS_ADMIN_API_KEY"] ?? "";

                // i use 5 km for now. do we have dropdown or number input for getting range of radius of where to search?
                var radiusKm = 5;

                // When searching, use auckland center with big radius so we get all station then filter it
                var geoEndpoint = searched
                    ? $"api/location/query?longitude=174.76&latitude=-36.85&radiusKm=100&key={Uri.EscapeDataString(apiKey)}"
                    : $"api/location/query?longitude={longitude}&latitude={latitude}&radiusKm={radiusKm}&key={Uri.EscapeDataString(apiKey)}";

                var geoClient = _httpClientFactory.CreateClient("GeoLocationApi");
                var geoResponse = await geoClient.GetAsync(geoEndpoint);

                if (geoResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ViewData["Results"] = new List<StationResult>();
                    return View();
                }
                else if (!geoResponse.IsSuccessStatusCode)
                {
                    ViewData["Error"] = $"API returned: {geoResponse.StatusCode}";
                    return View();
                }

                var results = await geoResponse.Content.ReadFromJsonAsync<List<StationResult>>() ?? new List<StationResult>();

                if (hasLocation) results = results.Where(r => (r.AddressString ?? "").Contains(locationTb!, StringComparison.OrdinalIgnoreCase) || $"{r.Brand} {r.Branch}".Contains(locationTb!, StringComparison.OrdinalIgnoreCase)).ToList();

                if (hasStation) results = results.Where(r => r.Brand != null && r.Brand.StartsWith(station!, StringComparison.OrdinalIgnoreCase)).ToList();

                // Get all the price in one go then match it by the station name since price search dont give back station id
                var priceClient = _httpClientFactory.CreateClient("GaaSApi");
                var priceEndpoint = $"price/search?key={Uri.EscapeDataString(apiKey)}&Code={_azKey}";
                if (hasFuelType) priceEndpoint += $"&fuelType={Uri.EscapeDataString(fuelType!)}";

                var priceResponse = await priceClient.GetAsync(priceEndpoint);
                var prices = priceResponse.IsSuccessStatusCode ? await priceResponse.Content.ReadFromJsonAsync<List<PriceResult>>() : new List<PriceResult>();

                foreach (var result in results)
                {
                    // Changes the image corresponding to the station's brand
                    result.Icon = (result.Brand?.ToLower()) switch
                    {
                        "z energy" => "/images/z.png",
                        "bp" => "/images/bp.png",
                        "caltex" => "/images/caltex.png",
                        _ => null
                    };
                    result.Prices = prices.Where(p => p.StationName == $"{result.Brand} {result.Branch}").ToList();
                }

                // If they pick a fuel type, the one without price for that fuel will not needed
                if (hasFuelType) results = results.Where(r => r.Prices.Count > 0).ToList();
                // Orders the result from lowest price to highest
                results = results
                    .OrderBy(r => r.Prices.Any() ? r.Prices.Min(p => p.PriceValue) : decimal.MaxValue)
                    .ToList();
                ViewData["Results"] = results;
                return View();
            }
            catch (Exception ex)
            {
                ViewData["Error"] = $"Error: {ex.Message}";
                return View();
            }
        }
    }
}
