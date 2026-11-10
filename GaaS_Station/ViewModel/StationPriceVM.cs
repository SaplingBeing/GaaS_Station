using GaaS_Station.Models;

namespace GaaS_Station.ViewModel
{
    public class StationPriceVM
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public List<FuelType> FuelTypes { get; set; } = new();
        public List<Price> Prices { get; set; } = new();
    }
}
