using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using ToTraveler.Auth.Model;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Swashbuckle.AspNetCore.Filters;

namespace ToTraveler.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Authentication endpoints for user management")]
    public class AuthController : ControllerBase
    {
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

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("/api/Register")]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user account with the provided username, email, and password"
        )]
        [SwaggerResponse(201, "User successfully created")]
        [SwaggerResponse(400, "Invalid username provided")]
        [SwaggerResponse(409, "Username already exists")]
        [Produces("application/json")]
        public async Task<IActionResult> Register([FromBody] RegiseterUserDto dto)
        {
            if (dto.UserName == null)
                return BadRequest("Invalid Username");

            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user != null)
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

        /// <summary>
        /// Authenticate user and get access token
        /// </summary>
        [HttpPost("/api/Login")]
        [SwaggerOperation(
            Summary = "Authenticate user",
            Description = "Authenticates user credentials and returns an access token"
        )]
        [SwaggerResponse(200, "Login successful", typeof(SuccessfulLoginUserDto))]
        [SwaggerResponse(400, "Invalid credentials")]
        [SwaggerResponse(409, "User does not exist")]
        [Produces("application/json")]
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
            };

            _httpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

            return Ok(new SuccessfulLoginUserDto(accessToken));
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("/api/AccessToken")]
        [SwaggerOperation(
            Summary = "Refresh access token",
            Description = "Creates a new access token using the refresh token stored in cookies"
        )]
        [SwaggerResponse(200, "New access token generated", typeof(SuccessfulLoginUserDto))]
        [SwaggerResponse(422, "Invalid or missing refresh token")]
        [Produces("application/json")]
        public async Task<IActionResult> AccessToken()
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

            var sessionIdAsGuid = Guid.Parse(sessionId);
            if (!await _sessionService.IsSessionValidAsync(sessionIdAsGuid, refreshToken))
            {
                return UnprocessableEntity();
            }

            var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
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
            };

            _httpContext.Response.Cookies.Append("RefreshToken", newRefreshToken, cookieOptions);

            await _sessionService.ExtendSessionAsync(sessionIdAsGuid, newRefreshToken, expiresAt);

            return Ok(new SuccessfulLoginUserDto(accessToken));
        }

        /// <summary>
        /// Logout user and invalidate session
        /// </summary>
        [HttpPost("/api/Logout")]
        [SwaggerOperation(
            Summary = "Logout user",
            Description = "Invalidates the current session and removes the refresh token cookie"
        )]
        [SwaggerResponse(200, "Successfully logged out")]
        [SwaggerResponse(422, "Invalid or missing refresh token")]
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

        [SwaggerSchema(Required = new[] { "userName", "email", "password" })]
        public record RegiseterUserDto(
            [SwaggerParameter(Description = "Username for the new account")]
            string UserName,

            [SwaggerParameter(Description = "Email address")]
            string Email,

            [SwaggerParameter(Description = "Password (min 8 characters)")]
            string Password);

        [SwaggerSchema(Required = new[] { "userName", "password" })]
        public record LoginUserDto(
            [SwaggerParameter(Description = "Username of the existing account")]
            string UserName,

            [SwaggerParameter(Description = "Account password")]
            string Password);

        [SwaggerSchema]
        public record SuccessfulLoginUserDto(
            [SwaggerParameter(Description = "JWT access token")]
            string AccessToken);
    }

    public class RegisterDtoExample : IExamplesProvider<AuthController.RegiseterUserDto>
    {
        public AuthController.RegiseterUserDto GetExamples()
        {
            return new AuthController.RegiseterUserDto(
                UserName: "john.doe",
                Email: "john.doe@example.com",
                Password: "CapitalLetterNumberAndSymbol(JohnDoe4!)"
            );
        }
    }

    public class LoginDtoExample : IExamplesProvider<AuthController.LoginUserDto>
    {
        public AuthController.LoginUserDto GetExamples()
        {
            return new AuthController.LoginUserDto(
                UserName: "john.doe",
                Password: "CapitalLetterNumberAndSymbol(JohnDoe4!)"
            );
        }
    }

    public class SuccessfulLoginDtoExample : IExamplesProvider<AuthController.SuccessfulLoginUserDto>
    {
        public AuthController.SuccessfulLoginUserDto GetExamples()
        {
            return new AuthController.SuccessfulLoginUserDto(
                AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJqdGkiOiJhYzY3MWI5NS00ODRhLTRmMDktOGNkZS04ZWI1N2RkZTlhMGUiLCJzdWIiOiI1Y2ZkMGZjZS0yNWQyLTRlZTQtYTk4ZC04ODdjNDI2OWVmMjYiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOlsiQWRtaW4iLCJVc2VyIl0sImV4cCI6MTczNDI3MzE0MSwiaXNzIjoiTGFpbWlzIiwiYXVkIjoiVHJ1c3RlZENsaWVudCJ9.Ev77_w9gQksd4SMAKmjHmvJrrrEP7JNjITdNK_Hs3T8"
            );
        }
    }
}