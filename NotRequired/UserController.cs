//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using System.Threading.Tasks;
//using ToTraveler.DTOs;
//using ToTraveler.Models;

//namespace ToTraveler.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UserController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public UserController(AppDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> Get()
//        {
//            var users = await _context.Users
//                .Include(u => u.Location_Lists)
//                .ToListAsync();
            
//            if(users == null || users.Count <= 0)
//                return NotFound();

//            return Ok(users);
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> Get(int id)
//        {
//            var user = await _context.Users.FindAsync(id);

//            if (user == null)
//                return NotFound();

//            return Ok(user);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Post([FromBody] UserDTO dto)
//        {
//            if(dto == null)
//                return BadRequest();

//            //Conflict validation
//            if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
//            {
//                return StatusCode(StatusCodes.Status409Conflict, "User already exist");
//            }

//            var user = new User(dto.Username, dto.Password, dto.Email, dto.Name, dto.Surname, dto.Role);

//            await _context.Users.AddAsync(user);
//            await _context.SaveChangesAsync();
               
//            return new ObjectResult(user) { StatusCode = StatusCodes.Status201Created };
//        }

//        [HttpPut]
//        public async Task<IActionResult> Put(int id, [FromBody] UserDTO dto)
//        {
//            if (dto == null)
//                return BadRequest();

//            var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == id);

//            if(user == null)
//                return NotFound();

//            user.Username = dto.Username;
//            user.Password = dto.Password;
//            user.Email = dto.Email;
//            user.Name = dto.Name;
//            user.Surname = dto.Username;
//            user.Role = dto.Role;

//            await _context.SaveChangesAsync();

//            return Ok(user);
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == id);

//            if(user == null)
//            {
//                return NotFound();
//            }

//            //Removing all the Location_Lists assosiated with the User
//            var location_lists = user.Location_Lists;
//            if (location_lists != null)
//            {
//                foreach (var location_list in location_lists)
//                {
//                    await Delete(location_list.ID);
//                }
//            }

//            _context.Users.Remove(user);
//            await _context.SaveChangesAsync();
//            return Ok(user);
//        }
//    }
//}
