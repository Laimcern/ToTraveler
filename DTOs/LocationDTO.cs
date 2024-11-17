using System.Text.Json.Serialization;

namespace ToTraveler.DTOs
{
    #nullable disable
    public class LocationDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int CategoryID { get; set; }
        public string Address { get; set; }
    }
}
