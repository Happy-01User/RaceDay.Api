using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetString("Role") == "Participant";
        }

        private IActionResult? RequireParticipant()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // GET: /Participant - Browse all events
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _api.SendAsync(HttpMethod.Get, "api/events");
            var events = await _api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();
            return View(events);
        }

        // GET: /Participant/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var evt = await _api.ReadAsync<EventDto>(response);
            if (evt == null) return NotFound();

            // Load categories for this event for the Enrol form
            var catResponse = await _api.SendAsync(HttpMethod.Get, "api/categories");
            var allCats = await _api.ReadAsync<List<CategoryDto>>(catResponse) ?? new List<CategoryDto>();
            ViewBag.Categories = allCats.Where(c => c.EventId == id).ToList();

            return View(evt);
        }

        // GET: /Participant/MyEnrolments
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            var check = RequireParticipant();
            if (check != null) return check;

            var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
            if (!response.IsSuccessStatusCode)
            {
                response = await _api.SendAsync(HttpMethod.Get, "api/enrolments");
            }

            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new List<EnrolmentDto>();
            return View(enrolments);
        }

        // POST: /Participant/Enrol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(CreateEnrolmentRequest model)
        {
            var check = RequireParticipant();
            if (check != null) return check;

            if (!ModelState.IsValid) return RedirectToAction("Details", new { id = model.EventId });

            var response = await _api.SendAsync(HttpMethod.Post, "api/enrolments", model);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not enrol. You may already be enrolled in this event.";
                return RedirectToAction("Details", new { id = model.EventId });
            }

            TempData["Success"] = "Enrolled successfully!";
            return RedirectToAction("MyEnrolments");
        }

        // POST: /Participant/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var check = RequireParticipant();
            if (check != null) return check;

            var response = await _api.SendAsync(HttpMethod.Delete, $"api/enrolments/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Enrolment cancelled.";
            else
                TempData["Error"] = "Could not cancel enrolment.";

            return RedirectToAction("MyEnrolments");
        }
    }
}