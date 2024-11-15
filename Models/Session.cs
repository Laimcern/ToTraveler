using System.ComponentModel.DataAnnotations;
using ToTraveler.Auth.Model;

namespace ToTraveler.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public string LastRefreshToken { get; set; }
        public DateTimeOffset InitiatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool isRevoked{ get; set; }

        [Required]
        public required string UserId{ get; set; }
        public User User{ get; set; }
    }
}
