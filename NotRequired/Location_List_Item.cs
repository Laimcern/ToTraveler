//using System.Text.Json.Serialization;

//namespace ToTraveler.Models
//{
//    public class Location_List_Item
//    {
//        public int ID { get; set; }
//        public DateTime Added { get; set; }
//        public DateTime? Visited { get; set; }
//        public string? Note { get; set; }
//        public int LocationID { get; set; }
        
//        [JsonIgnore]
//        public virtual Location? Location { get; set; }
//        public int Location_ListID { get; set; }
        
//        [JsonIgnore]
//        public virtual Location_List? Location_List { get; set; }
//        public Location_List_Item() { }

//        public Location_List_Item(DateTime added, DateTime visited, string? note, int locationID, int location_ListID)
//        {
//            Added = added;
//            Visited = visited;
//            Note = note;
//            LocationID = locationID;
//            Location_ListID = location_ListID;
//        }
//    }
//}
