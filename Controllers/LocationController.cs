using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using ToTraveler.Auth.Model;
using ToTraveler.DTOs;
using ToTraveler.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ToTraveler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpContext? _httpContext;

        public LocationController(AppDbContext context, HttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContext = httpContextAccessor.HttpContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            // If Admin return all
            if (_httpContext.User.IsInRole(UserRoles.Admin))
            {
                var allLocations = await _context.Locations
                .Include(l => l.Category)
                .Include(l => l.User)
                .Include(l => l.Reviews!)
                    .ThenInclude(r => r.User)
                .ToListAsync();
                return Ok(allLocations);
            }

            // If not Admin
            var locations = await _context.Locations
            .Include(l => l.Category)
            .Include(l => l.User)
            .Include(l => l.Reviews!
                .Where(r => r.IsPrivate == false || r.UserId == userId))
                .ThenInclude(r => r.User)
            .ToListAsync();

            if (locations == null || locations.Count() <= 0)
                return NotFound();

            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            // If Admin return all
            if (_httpContext.User.IsInRole(UserRoles.Admin))
            {
                var allLocation = await _context.Locations
                .Include(l => l.Category)
                .Include(l => l.Reviews!)
                    .ThenInclude(r => r.User)
                .ToListAsync();
                return Ok(allLocation);
            }

            // If not Admin
            var location = await _context.Locations
                .Include(l => l.Category)
                .Include(l => l.Reviews!
                    .Where(r => r.IsPrivate == false || r.UserId == userId))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(loc => loc.ID == id);

            if (location == null)
                return NotFound();

            return Ok(location);
        }

        [HttpGet("{location_id}/reviews")]
        public async Task<IActionResult> GetLocationReviews(int location_id)
        {
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var location = await _context.Locations.FindAsync(location_id);

            if (location == null)
                return NotFound("Location does not exist");

            if (_httpContext.User.IsInRole(UserRoles.Admin))
            {
                var allReviews = await _context.Reviews
                    .Where(r => r.LocationID == location.ID)
                    .ToListAsync();

                if (allReviews == null || allReviews.Count() <= 0)
                    return NotFound("Location does not have Reviews");

                return Ok(allReviews);
            }

            var reviews = await _context.Reviews
                .Where(r => r.LocationID == location.ID)
                .Where(r => r.IsPrivate == false || r.UserId == userId)
                .ToListAsync();

            if (reviews == null || reviews.Count() <= 0)
                return NotFound("Location does not have Reviews");

            return Ok(reviews);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.User)]
        public async Task<IActionResult> Post([FromBody] LocationDTO dto)
        {
            var validation = await IsValid(dto);
            if (validation is not OkResult)
                return validation;

            if (await _context.LocationCategories.FirstOrDefaultAsync(x => x.ID == dto.CategoryID) is null)
                return NotFound("Location Category does not exist");

            //Authorization
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null)
                return Unauthorized();

            var location = new Location(dto.Title, dto.Description, dto.Latitude, dto.Longitude, dto.CategoryID, dto.Address, userId);

            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            return new ObjectResult(location) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody] LocationDTO dto)
        {
            var validation = await IsValid(dto);
            if (validation is not OkResult)
                return validation;

            var location = await _context.Locations.FirstOrDefaultAsync(u => u.ID == id);
            if (location == null)
                return NotFound();

            //Authorization
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!_httpContext.User.IsInRole(UserRoles.Admin) && userId != location.UserId)
                return Forbid();

            location.Title = dto.Title;
            location.Description = dto.Description;
            location.Latitude = dto.Latitude;
            location.Longitude = dto.Longitude;
            location.CategoryID = dto.CategoryID;
            location.Address = dto.Address;

            await _context.SaveChangesAsync();

            return Ok(location);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(u => u.ID == id);
            if (location == null)
            {
                return NotFound();
            }

            //Authorization
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!_httpContext.User.IsInRole(UserRoles.Admin) && userId != location.UserId)
                return Forbid();

            //Removing all the Reviews assosiated with the Location
            var reviews = location.Reviews;
            if (reviews != null)
            {
                foreach (var review in reviews)
                {
                    _context.Reviews.Remove(review);
                }
            }

            //Removing the location
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return Ok(location);
        }

        private async Task<IActionResult> IsValid(LocationDTO dto)
        {
            if (dto == null)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Invalid Title");

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest("Invalid Description");

            if (string.IsNullOrWhiteSpace(dto.Address))
                return BadRequest("Invalid Address");

            var category = await _context.LocationCategories.FirstOrDefaultAsync(lc => lc.ID == dto.CategoryID);
            if (category == null)
                return NotFound("Category does not exist");

            return Ok();
        }
    }

}
