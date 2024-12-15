using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ToTraveler.Auth.Model;

namespace ToTraveler.Models
{
    public class Review
    {
        public int ID { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Text { get; set; }
        public bool IsPrivate { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public int LocationID { get; set; }
        //public virtual Location? Location { get; set; }

        public Review()
        {
            
        }

        public Review(int rating, string? text, bool isPrivate, string userID, int locationID)
        {
            Rating = rating;
            Text = text;
            IsPrivate = isPrivate;
            UserId = userID;
            LocationID = locationID;
        }
    }
}
