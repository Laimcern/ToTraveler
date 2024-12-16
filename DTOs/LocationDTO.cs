using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace ToTraveler.DTOs
{
#nullable disable
    [SwaggerSchema(Required = new[] { "title", "description", "latitude", "longitude", "categoryId", "address" })]
    public class LocationDTO
    {
        public LocationDTO(string Title, string Description, double Latitude, double Longitude, int CategoryID, string Address)
        {
            this.Title = Title;
            this.Description = Description;
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.CategoryID = CategoryID;
            this.Address = Address;
        }

        [SwaggerParameter(Description = "Title of the location (required, non-empty)")]
        public string Title { get; set; }

        [SwaggerParameter(Description = "Detailed description of the location (required, non-empty)")]
        public string Description { get; set; }

        [SwaggerParameter(Description = "Latitude coordinate of the location")]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [SwaggerParameter(Description = "Longitude coordinate of the location")]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [SwaggerParameter(Description = "ID of the location category")]
        public int CategoryID { get; set; }

        [SwaggerParameter(Description = "Physical address of the location (required, non-empty)")]
        public string Address { get; set; }
    }
}
