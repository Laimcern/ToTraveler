using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ToTraveler.Auth.Model;
using ToTraveler.DTOs;
using ToTraveler.Models;
using Swashbuckle.AspNetCore.Annotations; // Add this for Swagger annotations

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
        [SwaggerResponse(200, "Successfully retrieved reviews.", typeof(List<Review>))]
        [SwaggerResponse(404, "No reviews found.")]
        public async Task<IActionResult> Get()
        {
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (_httpContext.User.IsInRole(UserRoles.Admin))
            {
                var allReviews = await _context.Reviews.ToListAsync();
                return Ok(allReviews);
            }

            var reviews = await _context.Reviews
                .Where(r => r.IsPrivate == false || r.UserId == userId)
                .Include(r => r.User)
                .ToListAsync();

            if (reviews == null || reviews.Count <= 0)
                return NotFound();

            return Ok(reviews);
        }

        [HttpGet("{id}")]
        [SwaggerResponse(200, "Successfully retrieved the review.", typeof(Review))]
        [SwaggerResponse(404, "Review not found.")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (_httpContext.User.IsInRole(UserRoles.Admin))
                return Ok(await _context.Reviews.FindAsync(id));

            var review = await _context.Reviews
                .Where(r => r.IsPrivate == false || r.UserId == userId)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (review == null)
                return NotFound();

            return Ok(review);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.User)]
        [SwaggerResponse(201, "Review created successfully.", typeof(Review))]
        [SwaggerResponse(400, "Bad request. Invalid data.")]
        [SwaggerResponse(404, "Location not found.")]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> Post([FromBody] ReviewDTO dto)
        {
            if (dto == null)
                return BadRequest();

            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null)
                return Unauthorized();

            if (!await _context.Locations.AnyAsync(l => l.ID == dto.LocationID))
                return BadRequest("Such Location does not exist");

            var review = new Review(dto.Rating, dto.Text, dto.IsPrivate, userId, dto.LocationID);

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return new ObjectResult(review) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("{id}")]
        [Authorize]
        [SwaggerResponse(200, "Review updated successfully.", typeof(Review))]
        [SwaggerResponse(400, "Bad request. Invalid data.")]
        [SwaggerResponse(404, "Review not found.")]
        [SwaggerResponse(403, "Forbidden. You do not have permission to modify this review.")]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> Put(int id, [FromBody] ReviewDTO_Put dto)
        {
            if (dto == null)
                return BadRequest();

            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (review == null)
                return NotFound();

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
        [SwaggerResponse(200, "Review deleted successfully.", typeof(Review))]
        [SwaggerResponse(404, "Review not found.")]
        [SwaggerResponse(403, "Forbidden. You do not have permission to delete this review.")]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(u => u.ID == id);

            if (review == null)
                return NotFound();

            var userId = _httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!_httpContext.User.IsInRole(UserRoles.Admin) && userId != review.UserId)
                return Forbid();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }
    }
}
