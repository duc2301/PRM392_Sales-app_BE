namespace Services.DTOs.ResponseDTOs
{
    public class StoreLocationDTO
    {
        public int LocationId { get; set; }
        public string Address { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
