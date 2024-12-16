using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;
using ToTraveler.Auth.Model;
using ToTraveler.DTOs;
using ToTraveler.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ToTraveler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Endpoints for managing location data")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        [SwaggerOperation(
            Summary = "Get all locations",
            Description = "Retrieves all locations. Admins see all data, regular users see only public reviews and their own private reviews"
        )]
        [ProducesResponseType(typeof(List<Location>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
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
        [SwaggerOperation(
           Summary = "Get location by ID",
           Description = "Retrieves a specific location by ID. Admins see all data, regular users see only public reviews and their own private reviews"
       )]
        [ProducesResponseType(typeof(Location), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
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
        [SwaggerOperation(
            Summary = "Get location reviews",
            Description = "Retrieves all reviews for a specific location. Admins see all reviews, regular users see only public reviews and their own private reviews"
        )]
        [ProducesResponseType(typeof(List<Review>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
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
        [SwaggerOperation(
            Summary = "Create new location",
            Description = "Creates a new location. Requires User role."
        )]
        [ProducesResponseType(typeof(Location), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LocationResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
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
        [SwaggerOperation(
             Summary = "Update location",
             Description = "Updates an existing location. Users can only update their own locations, admins can update any location."
         )]
        [ProducesResponseType(typeof(Location), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LocationResponseExample))] 
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExample))] 
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenResponseExample))] 
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))] 
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
        [Authorize]
        [SwaggerOperation(
    Summary = "Delete location",
    Description = "Deletes a location and all its associated reviews. Users can only delete their own locations, admins can delete any location."
)]
        [ProducesResponseType(typeof(Location), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LocationResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
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
    public class ForbiddenResponseExample : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
        {
            return new ProblemDetails
            {
                Title = "Forbidden",
                Status = 403,
            };
        }
    }
    public class InternalServerErrorResponseExample : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
        {
            return new ProblemDetails
            {
                Title = "Internal server error",
                Status = 500,
            };
        }
    }
    public class UnauthorizedResponseExample : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
        {
            return new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
            };
        }
    }

    public class NotFoundResponseExample : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
        {
            return new ProblemDetails
            {
                Title = "Location Not Found",
                Status = 404,
            };
        }
    }

    public class BadRequestResponseExample : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
        {
            return new ProblemDetails
            {
                Title = "Bad request",
                Status = 400,
            };
        }
    }

    public class LocationDtoExample : IExamplesProvider<LocationDTO>
    {
        public LocationDTO GetExamples()
        {
            return new LocationDTO(
                Title: "Eiffel Tower",
                Description: "Famous landmark in Paris, France",
                Latitude: 48.858372,
                Longitude: 2.294481,
                CategoryID: 1,
                Address: "Champ de Mars, 5 Avenue Anatole France, 75007 Paris, France"
            );
        }
    }

    public class LocationResponseExample : IExamplesProvider<Location>
    {
        public Location GetExamples()
        {
            return new Location(
                title: "Eiffel Tower",
                description: "Famous landmark in Paris, France",
                latitude: 48.858372,
                longitude: 2.294481,
                categoryID: 1,
                address: "Champ de Mars, 5 Avenue Anatole France, 75007 Paris, France",
                userId: "user123"
            )
            {
                ID = 1,
                Category = new LocationCategory("Landmarks"),
                Reviews = new List<Review>
            {
                new Review
                {
                    ID = 1,
                    Rating = 5,
                    IsPrivate = false,
                    UserId = "user456",
                    User = new User { UserName = "john.doe" }
                }
            }
            };
        }
    }
}

