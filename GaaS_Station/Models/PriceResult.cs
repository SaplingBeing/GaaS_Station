namespace GaaS_Station.Models
{
    public class PriceResult
    {
        public int PriceID { get; set; }
        public string? StationName { get; set; }
        public string? FuelType { get; set; }
        public decimal PriceValue { get; set; }
    }
}
