using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ToTraveler.DTOs
{
    public class ReviewDTO
    {

        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Text { get; set; }
        public bool IsPrivate { get; set; }
        public string UserID { get; set; }
        public int LocationID { get; set; }
    }
}
