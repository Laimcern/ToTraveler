using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ToTraveler.Models
{
    public class Location
    {
        public int ID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int CategoryID { get; set; }
        public virtual Location_Category? Category { get; set; }
        public string? Address { get; set; }

        [Required]
        public string UserId { get; set; }
        //public virtual User? User { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        public Location(string? title, string? description, double latitude, double longitude, int categoryID, string? address, string userId)
        {
            Title = title;
            Description = description;
            Latitude = latitude;
            Longitude = longitude;
            CategoryID = categoryID;
            Address = address;
            UserId = userId;
        }
    }
}
