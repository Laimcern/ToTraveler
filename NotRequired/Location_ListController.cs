//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.JSInterop.Infrastructure;
//using ToTraveler.DTOs;
//using ToTraveler.Models;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace ToTraveler.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class Location_ListController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public Location_ListController(AppDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> Get()
//        {
//            var location_lists = await _context.Location_Lists
//                .Include(ll => ll.Items)
//                .ToListAsync();

//            if (location_lists == null || location_lists.Count <= 0)
//                return NotFound();

//            return Ok(location_lists);
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> Get(int id)
//        {
//            var location_list = await _context.Location_Lists
//                .Include(ll => ll.Items)
//                .FirstOrDefaultAsync(loc => loc.ID == id);

//            if (location_list == null)
//                return NotFound();

//            return Ok(location_list);
//        }

//        [HttpPost("Item")]
//        public async Task<IActionResult> PostItem([FromBody] Location_List_ItemDTO dto)
//        {
//            //Validation
//            if (dto == null)
//            {
//                return BadRequest();
//            }
            
//            var location = await _context.Locations.FirstOrDefaultAsync(l => l.ID == dto.LocationID);
//            if (location == null)
//            {
//                return NotFound("Location does not exist");
//            }
            
//            var location_list = await _context.Location_Lists
//                .Include(ll => ll.Items)
//                .FirstOrDefaultAsync(l => l.ID == dto.Location_ListID);
//            if (location_list == null)
//            {
//                return NotFound("Location_List does not exist");
//            }

//            //Checking for dublicate Items
//            if(location_list.Items != null)
//            {
//                foreach (var location_item in location_list.Items)
//                {
//                    if (location_item.LocationID == dto.LocationID) 
//                    {
//                        return Conflict("Location in the Location_List already exists");
//                    }
//                }
//            }

//            var added = DateTime.Today;
//            var visited = DateTime.Today;

//            var item = new Location_List_Item(added, visited, dto.Note, dto.LocationID, dto.Location_ListID);
            
//            await _context.Location_List_Items.AddAsync(item);
//            await _context.SaveChangesAsync();

//            return new ObjectResult(dto) { StatusCode = StatusCodes.Status201Created };
//        }

//        [HttpPost]
//        public async Task<IActionResult> Post([FromBody] Location_ListDTO dto)
//        {
//            //Validation
//            if (dto == null)
//            {
//                return BadRequest();
//            }
//            if (dto.Name == null || dto.Name.Length <= 0)
//            {
//                return BadRequest("Invalid Name");
//            }


//            ////Checking if such User exists
//            //var user = await _context.Users
//            //    .Include(u => u.Location_Lists)
//            //    .FirstOrDefaultAsync(u => u.ID == dto.UserID);
//            //if (user == null)
//            //{
//            //    return NotFound("User does not exist");
//            //}

//            ////Checking if payload Name is unique across Users Location_Lists
//            //if(user.Location_Lists != null)
//            //{
//            //    foreach (var list in user.Location_Lists.ToList())
//            //    {
//            //        if (list.Name == dto.Name)
//            //        {
//            //            return Conflict("Location_List with such Name already exist");
//            //        }
//            //    }
//            //}

//            var location_list = new Location_List(dto.Name, dto.UserID);

//            await _context.Location_Lists.AddAsync(location_list);
//            await _context.SaveChangesAsync();

//            return new ObjectResult(dto) { StatusCode = StatusCodes.Status201Created };
//        }

//        [HttpPut("{id}")]
//        public async Task<IActionResult> Put(int id, [FromBody] Location_ListDTO dto)
//        {
//            //Checking if request Location_List exists
//            var location_list = await _context.Location_Lists.FirstOrDefaultAsync(u => u.ID == id);
//            if (location_list == null)
//            {
//                return NotFound("Location_List does not exist");
//            }

//            //var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == dto.UserID);
//            //if (user == null)
//            //{
//            //    return NotFound("User does not exist");
//            //}

//            //Updating attributes
//            location_list.Name = dto.Name;

//            await _context.SaveChangesAsync();

//            return Ok(location_list);
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var location_list = await _context.Location_Lists.FirstOrDefaultAsync(u => u.ID == id);

//            if (location_list == null)
//            {
//                return NotFound();
//            }

//            //Removing all Location_List Items
//            var items = await _context.Location_List_Items
//                .Where(u => u.Location_ListID == id)
//                .ToListAsync();
//            foreach (var item in items)
//            {
//                _context.Location_List_Items.Remove(item);
//            }

//            //Removing the Location_List
//            _context.Location_Lists.Remove(location_list);
//            await _context.SaveChangesAsync();
//            return Ok(location_list);
//        }
//    }
//}
