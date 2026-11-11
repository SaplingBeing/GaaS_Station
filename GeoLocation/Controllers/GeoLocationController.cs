using GeoLocation.Key;
using GeoLocation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoLocation.Controllers;

[Route("api/location")]
[ApiController]
public class GeoLocationController : ControllerBase
{
    private readonly ILogger<GeoLocationController> _logger;
    private readonly DatabaseContext _dbContext;
    private readonly ValidateKey _validateKey;

    public GeoLocationController(ILogger<GeoLocationController> logger, DatabaseContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _validateKey = new ValidateKey(dbContext);
    }

    [HttpGet("query")]
    [HttpPost("query")]
    async public Task<IActionResult> Run()
    {
        var req = Request;

        // Establish variables to hold the query parameters
        decimal? longitude;
        decimal? latitude;
        decimal? radiusKm;
        string apiKey;

        // Variables to hold values later
        object result;

        // Determine if the request is a GET or POST and extract the parameters accordingly
        if (req.Method == "GET")
        {
            longitude = decimal.TryParse(req.Query["longitude"].ToString(), out decimal longitudeValue) ? longitudeValue : null;
            latitude = decimal.TryParse(req.Query["latitude"].ToString(), out decimal latitudeValue) ? latitudeValue : null;
            radiusKm = decimal.TryParse(req.Query["radiusKm"].ToString(), out decimal radiusValue) ? radiusValue : null;
            apiKey = req.Query["key"].ToString() ?? string.Empty;
        }
        else if (req.Method == "POST")
        {
            var data = await req.ReadFromJsonAsync<LocationRequestParams>();
            if (data == null) return new BadRequestObjectResult("Invalid request body");

            longitude = data.Longitude;
            latitude = data.Latitude;
            radiusKm = data.RadiusKm;
            apiKey = data.Key ?? string.Empty;
        }
        else
        {
            return new BadRequestObjectResult("Invalid request method");
        }

        // Validate the API key and admin access
        var accessError = await _validateKey.ValidateAdminAccessAsync(apiKey);
        if (accessError != null) return accessError;

        // Validate the location parameters
        if (!longitude.HasValue || !latitude.HasValue || !radiusKm.HasValue) return new BadRequestObjectResult("Longitude, latitude and radiusKm are required");

        if (longitude < -180 || longitude > 180) return new BadRequestObjectResult("Longitude must be between -180 and 180");
        if (latitude < -90 || latitude > 90) return new BadRequestObjectResult("Latitude must be between -90 and 90");
        if (radiusKm <= 0) return new BadRequestObjectResult("Radius must be greater than zero");

        // Retrieve stations, their corresponding geolocation records and their brands
        var stations = await
            (from s in _dbContext.Stations
             join g in _dbContext.GeoLocations on s.GeoLocation equals g.GeoLocationID
             join b in _dbContext.Brands on s.Brand equals b.BrandID
             select new
             {
                 Station = s,
                 GeoLocation = g,
                 Brand = b
             })
            .ToListAsync();

        // Only stations within the specified radius are returned, ordered from nearest to farthest.
        List<LocationResponse> alldata;

        alldata = (
            from x in stations
            let distance = CalculateDistance(
                (double)latitude.Value,
                (double)longitude.Value,
                (double)x.GeoLocation.Latitude,
                (double)x.GeoLocation.Longitude
            )
            where distance <= (double)radiusKm.Value
            orderby distance
            select new LocationResponse
            {
                StationID = x.Station.StationID,
                Brand = x.Brand.BrandName,
                Branch = x.Station.Branch,
                AddressString = x.GeoLocation.AddressString,
                Longitude = x.GeoLocation.Longitude,
                Latitude = x.GeoLocation.Latitude
            }
        ).ToList();

        if (alldata.Count == 0) return new NotFoundObjectResult("No stations found within the specified radius");

        result = alldata;
        return new OkObjectResult(result);
    }

    // Convert degrees into radians because the Haversine formula uses radians for trigonometric calculations
    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    // Calculate the distance between two geographical coordinates using the Haversine formula
    private static double CalculateDistance(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        /*
         * source: https://www.movable-type.co.uk/scripts/latlong.html
         * Haversine formula:
         * a = sin²(Δφ / 2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ / 2)
         * c = 2 ⋅ atan2( √a, √(1−a) )
         * D = R ⋅ c
         */

        const double earthRadius = 6371;

        double latitudeDifference = ToRadians(latitude2 - latitude1);
        double longitudeDifference = ToRadians(longitude2 - longitude1);

        double a = Math.Pow(Math.Sin(latitudeDifference / 2), 2) +
                   Math.Cos(ToRadians(latitude1)) *
                   Math.Cos(ToRadians(latitude2)) *
                   Math.Pow(Math.Sin(longitudeDifference / 2), 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Multiply the result by the Earth's radius to get the distance in kilometers
        return earthRadius * c;
    }
}

public class LocationRequestParams
{
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? RadiusKm { get; set; }
    public string? Key { get; set; }
}

public class LocationResponse
{
    public int StationID { get; set; }
    public string? Brand { get; set; }
    public string? Branch { get; set; }
    public string? AddressString { get; set; }
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
}