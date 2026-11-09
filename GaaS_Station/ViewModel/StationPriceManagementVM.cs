namespace GaaS_Station.ViewModel
{
    public class StationPriceManagementVM
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public List<FuelTypeItemVM> FuelTypes { get; set; } = new();
        public List<StationPriceItemVM> Prices { get; set; } = new();
    }
}
