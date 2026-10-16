using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // ---------- REGISTER ----------

        /// <summary>
        /// Registers a new user as either an Organiser or a Participant.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ---------- LOGIN ----------

        /// <summary>
        /// Authenticates a user and creates a session with their UserId and Role.
        /// </summary>
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Store session info
            HttpContext.Session.SetInt32("UserId", result.UserId);
            HttpContext.Session.SetString("Role", result.Role);
            HttpContext.Session.SetString("Email", result.Email);

            return Ok(result);
        }

        // ---------- LOGOUT ----------
        /// <summary>
        /// Clears the current session and logs the user out.
        /// </summary>

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }

        // ---------- WHO AM I ----------
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");
            var email = HttpContext.Session.GetString("Email");

            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            return Ok(new { userId, role, email });
        }
    }
}