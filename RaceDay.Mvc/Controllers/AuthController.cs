using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;

        public AuthController(IConfiguration config, IHttpClientFactory httpFactory)
        {
            _config = config;
            _httpFactory = httpFactory;
        }

        private string ApiBase => _config["ApiSettings:BaseUrl"]
            ?? _config["RaceDayApi:BaseUrl"]
            ?? "https://localhost:7001";

        // ---------- LOGIN ----------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = _httpFactory.CreateClient();
            var json = JsonSerializer.Serialize(new { email, password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{ApiBase}/api/Auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);

            string token = result.TryGetProperty("token", out var t) ? t.GetString() ?? "" : "";
            string role = result.TryGetProperty("role", out var r) ? r.GetString() ?? "" : "";
            string userEmail = result.TryGetProperty("email", out var e) ? e.GetString() ?? email : email;

            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("UserEmail", userEmail);

            if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Organiser");

            return RedirectToAction("Index", "Participant");
        }

        // ---------- REGISTER ----------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string role)
        {
            var client = _httpFactory.CreateClient();
            var json = JsonSerializer.Serialize(new { fullName, email, password, role });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{ApiBase}/api/Auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Registration failed. Try a different email.";
                return View();
            }

            return RedirectToAction("Login");
        }

        // ---------- LOGOUT ----------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}