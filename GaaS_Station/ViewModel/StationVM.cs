namespace GaaS_Station.ViewModel
{
    public class StationVM
    {
        public int StationId { get; set; }
        public string Branch { get; set; }
        public string BrandName { get; set; }
        public int BrandId { get; set; }
        public string AddressString { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // List of existing prices associated with this station
        public List<StationPriceManagementVM> Prices { get; set; } = new();
    }
}
