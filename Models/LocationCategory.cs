namespace ToTraveler.Models
{
    public class LocationCategory
    {
        public int ID { get; set; }
        public string Name { get; set; } = String.Empty;

        public LocationCategory(string name)
        {
            Name = name;
        }
    }
}
