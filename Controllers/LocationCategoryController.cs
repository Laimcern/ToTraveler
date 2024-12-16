using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop.Infrastructure;
using ToTraveler.Auth.Model;
using ToTraveler.DTOs;
using ToTraveler.Models;
using Swashbuckle.AspNetCore.Annotations; // Add this for Swagger annotations

namespace ToTraveler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationCategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerResponse(200, "Successfully retrieved list of location categories.", typeof(List<LocationCategory>))]
        [SwaggerResponse(404, "No location categories found.")]
        public async Task<IActionResult> Get()
        {
            var LocationCategories = await _context.LocationCategories
                .ToListAsync();

            if (LocationCategories == null || LocationCategories.Count <= 0)
                return NotFound();

            return Ok(LocationCategories);
        }

        [HttpGet("{id}")]
        [SwaggerResponse(200, "Successfully retrieved location category.", typeof(LocationCategory))]
        [SwaggerResponse(404, "Location category not found.")]
        public async Task<IActionResult> Get(int id)
        {
            var location_category = await _context.LocationCategories
                .FirstOrDefaultAsync(loc => loc.ID == id);

            if (location_category == null)
                return NotFound();

            return Ok(location_category);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [SwaggerResponse(201, "Location category created successfully.", typeof(LocationCategory))]
        [SwaggerResponse(400, "Bad request. Invalid data.")]
        [SwaggerResponse(409, "Location category with the given name already exists.")]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> Post([FromBody] string name)
        {
            // Validation
            if (name.IsNullOrEmpty())
                return BadRequest();

            var location_category = new LocationCategory(name);
            if (await _context.LocationCategories.FirstOrDefaultAsync(lc => lc.Name == name) is not null)
                return Conflict("Location Category with such name already exists");

            await _context.LocationCategories.AddAsync(location_category);
            await _context.SaveChangesAsync();

            return new ObjectResult(location_category) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        [SwaggerResponse(200, "Location category updated successfully.", typeof(LocationCategory))]
        [SwaggerResponse(400, "Bad request. Invalid data.")]
        [SwaggerResponse(404, "Location category not found.")]
        [SwaggerResponse(409, "Location category with the given name already exists.")]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> Put(int id, [FromBody] string name)
        {
            // Validation
            if (name.IsNullOrEmpty())
                return BadRequest();

            var location_category = await _context.LocationCategories.FirstOrDefaultAsync(u => u.ID == id);
            if (location_category == null)
                return NotFound("Location_List does not exist");

            if (await _context.LocationCategories.FirstOrDefaultAsync(lc => lc.Name == name) is not null)
                return Conflict("Location Category with such name already exists");

            // Updating attributes
            location_category.Name = name;
            await _context.SaveChangesAsync();

            return Ok(location_category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        [SwaggerResponse(200, "Location category deleted successfully.", typeof(LocationCategory))]
        [SwaggerResponse(404, "Location category not found.")]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> Delete(int id)
        {
            var LocationCategories = await _context.LocationCategories.FirstOrDefaultAsync(u => u.ID == id);

            if (LocationCategories == null)
                return NotFound();

            // Removing the Location_List
            _context.LocationCategories.Remove(LocationCategories);
            await _context.SaveChangesAsync();

            return Ok(LocationCategories);
        }
    }
}
