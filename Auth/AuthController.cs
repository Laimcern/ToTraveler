using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using ToTraveler.Auth.Model;

namespace ToTraveler.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly HttpContext? _httpContext;
        private readonly SessionService _sessionService;

        public AuthController(UserManager<User> userManager, JwtTokenService jwtTokenService, IHttpContextAccessor httpContextAccessor, SessionService sessionService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _httpContext = httpContextAccessor.HttpContext;
            _sessionService = sessionService;
        }

        [HttpPost("/api/Register")]
        public async Task<IActionResult> Register([FromBody] RegiseterUserDto dto)
        {
            if (dto.UserName == null)
                return BadRequest("Invalid Username");

            var user = await _userManager.FindByNameAsync(dto.UserName);
            if(user != null)
                return Conflict("Username already exists");

            var newUser = new User()
            { 
                UserName = dto.UserName,
                Email = dto.Email,
            };

            var createUserResult = await _userManager.CreateAsync(newUser, dto.Password);
            if (!createUserResult.Succeeded)
                return Conflict(createUserResult.Errors);

            await _userManager.AddToRoleAsync(newUser, UserRoles.User);

            return Created();
        }

        [HttpPost("/api/Login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            if (dto.UserName == null)
                return BadRequest("Invalid Username");

            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
                return Conflict("User does not exists");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                return BadRequest("Password or Username was inccorect");

            var roles = await _userManager.GetRolesAsync(user);

            var sessionId = Guid.NewGuid();
            var expiresAt = DateTime.UtcNow.AddDays(1);
            var accessToken = _jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
            var refreshToken = _jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

            await _sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, expiresAt);

            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                //Secure = false, //Should be true they said
            };

            _httpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

            return Ok(new SuccessfulLoginUserDto(accessToken));
        }

        [HttpPost("/api/AccessToken")]
        public async Task<IActionResult> AccessToken()
        {
            if(!_httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return UnprocessableEntity();
            }

            if(!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            {
                return UnprocessableEntity();
            }

            var sessionId = claims.FindFirstValue("SessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                return UnprocessableEntity();
            }

            var sessionIdAsGuid = Guid.Parse(sessionId);
            if(!await _sessionService.IsSessionValidAsync(sessionIdAsGuid, refreshToken))
            {
                return UnprocessableEntity();
            }

            var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return UnprocessableEntity();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var expiresAt = DateTime.UtcNow.AddDays(1);
            var accessToken = _jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
            var newRefreshToken = _jwtTokenService.CreateRefreshToken(sessionIdAsGuid, user.Id, expiresAt);

            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                //Secure = false, //Should be true they said
            };

            _httpContext.Response.Cookies.Append("RefreshToken", newRefreshToken, cookieOptions);

            await _sessionService.ExtendSessionAsync(sessionIdAsGuid, newRefreshToken, expiresAt);

            return Ok(new SuccessfulLoginUserDto(accessToken));
        }

        [HttpPost("/api/Logout")]
        public async Task<IActionResult> Logout()
        {
            if (!_httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return UnprocessableEntity();
            }

            if (!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            {
                return UnprocessableEntity();
            }

            var sessionId = claims.FindFirstValue("SessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                return UnprocessableEntity();
            }

            await _sessionService.InvalidateSessionAsync(Guid.Parse(sessionId));

            _httpContext.Response.Cookies.Delete("RefreshToken");

            return Ok();
        }

        public record RegiseterUserDto(string UserName, string Email,  string Password);
        public record LoginUserDto(string UserName, string Password);
        public record SuccessfulLoginUserDto(string AccessToken);
    } 
}
