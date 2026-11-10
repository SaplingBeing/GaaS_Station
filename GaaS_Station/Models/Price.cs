namespace GaaS_Station.Models
{
    public class Price
    {
        public int PriceID { get; set; }
        public int Station { get; set; }
        public int FuelTypeId { get; set; }
        public string? FuelType { get; set; }
        public decimal PriceValue { get; set; }
    }
}
