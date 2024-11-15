//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//using ToTraveler.Models;

//namespace ToTraveler
//{
//    public class AppDbContext : IdentityDbContext<User>
//    {
//        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//        {

//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            //modelBuilder.Entity<Location_List_Item>()
//            //    .HasKey(li => new { li.LocationID, li.Location_ListID });

//            //modelBuilder.Entity<Location_List_Item>()
//            //    .HasOne(li => li.Location)
//            //    .WithMany(l => l.Items)
//            //    .HasForeignKey(li => li.LocationID);

//            //modelBuilder.Entity<Location_List_Item>()
//            //    .HasOne(li => li.Location_List)
//            //    .WithMany(l => l.Items)
//            //    .HasForeignKey(li => li.Location_ListID);

//            base.OnModelCreating(modelBuilder);
//        }

//        //public DbSet<User> Users { get; set; }
//        public DbSet<Location> Locations { get; set; }
//        //public DbSet<Location_List> Location_Lists { get; set; }
//        //public DbSet<Location_List_Item> Location_List_Items { get; set; }
//        public DbSet<Location_Category> Location_Categories { get; set; }
//        public DbSet<Review> Reviews { get; set; }
//    }
//}
