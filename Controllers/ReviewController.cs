using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var reviews = await _context.Reviews.ToListAsync();

            if (reviews == null || reviews.Count <= 0)
                return NotFound();

            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return NotFound();

            return Ok(review);
        }

        

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReviewDTO dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            ////Checking if such User exists
            //if (!await _context.Users.AnyAsync(u => u.ID == dto.UserID))
            //{
            //    return BadRequest("Such User does not exist");
            //}

            //Checking if such Location exists
            if (!await _context.Locations.AnyAsync(l => l.ID == dto.LocationID))
            {
                return BadRequest("Such Location does not exist");
            }

            var review = new Review(dto.Rating, dto.Text, dto.IsPrivate, dto.UserID, dto.LocationID);

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return new ObjectResult(dto) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ReviewDTO_Put dto)
        {
            if (dto == null)
                return BadRequest();

            var review = await _context.Reviews.FirstOrDefaultAsync(u => u.ID == id);
            if (review == null)
            {
                return NotFound();
            }

            review.Rating = dto.Rating;
            review.IsPrivate = dto.IsPrivate;
            review.Text = dto.Text;
            await _context.SaveChangesAsync();
            
            return Ok(review);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(u => u.ID == id);

            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }
    }
}
