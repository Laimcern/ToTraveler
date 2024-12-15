using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ToTraveler.Auth.Model;
using ToTraveler.DTOs;
using ToTraveler.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ToTraveler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpContext? _httpContext;

        public ReviewController(AppDbContext context, HttpContextAccessor httpContextAccessor)
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
                var allReviews = await _context.Reviews.ToListAsync();
                return Ok(allReviews);
            }

            // If Guest return not private Reviews, if User also return User private Reviews
            var reviews = await _context.Reviews
                .Where(r => r.IsPrivate == false || r.UserId == userId)
                .Include(r => r.User)
                .ToListAsync();

            if (reviews == null || reviews.Count <= 0)
                return NotFound();

            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            // If Admin all reviews are possible to get
            if (_httpContext.User.IsInRole(UserRoles.Admin))
                return Ok(await _context.Reviews.FindAsync(id));

            // If Guest only not private Reviews ara gettable, User all owned Reviews are gettable
            var review = await _context.Reviews
                .Where(r => r.IsPrivate == false || r.UserId == userId)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (review == null)
                return NotFound();

            return Ok(review);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.User)]
        public async Task<IActionResult> Post([FromBody] ReviewDTO dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null)
                return Unauthorized();

            //Checking if such Location exists
            if (!await _context.Locations.AnyAsync(l => l.ID == dto.LocationID))
            {
                return BadRequest("Such Location does not exist");
            }

            var review = new Review(dto.Rating, dto.Text, dto.IsPrivate, userId, dto.LocationID);

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return new ObjectResult(review) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, [FromBody] ReviewDTO_Put dto)
        {
            if (dto == null)
                return BadRequest();

            var review = await _context.Reviews
                .Include(r=>r.User)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (review == null)
                return NotFound();

            //Authorization
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!_httpContext.User.IsInRole(UserRoles.Admin) && userId != review.UserId)
                return Forbid();

            review.Rating = dto.Rating;
            review.IsPrivate = dto.IsPrivate;
            review.Text = dto.Text;
            await _context.SaveChangesAsync();

            return Ok(review);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(u => u.ID == id);

            if (review == null)
                return NotFound();

            //Authorization
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!_httpContext.User.IsInRole(UserRoles.Admin) && userId != review.UserId)
                return Forbid();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }
    }
}
