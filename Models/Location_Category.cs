using Microsoft.AspNetCore.Mvc;

namespace ToTraveler.Models
{
    public class Location_Category
    {
        public int ID { get; set; }
        public string? Name { get; set; }

        public Location_Category(string name)
        {
            Name = name;
        }
    }
}
