namespace GaaS_Station.Models
{
    public class StationResult
    {
        public int StationID { get; set; }
        public string? Brand { get; set; }
        public string? Branch { get; set; }
        public string? AddressString { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public string? Icon { get; set; }
        public List<PriceResult> Prices { get; set; } = new List<PriceResult>();
    }
}
