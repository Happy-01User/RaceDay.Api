using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser";
        }

        private IActionResult? RequireOrganiser()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // GET: /Organiser
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var check = RequireOrganiser();
            if (check != null) return check;

            var response = await _api.SendAsync(HttpMethod.Get, "api/events/mine");
            if (!response.IsSuccessStatusCode)
            {
                response = await _api.SendAsync(HttpMethod.Get, "api/events");
            }

            var events = await _api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();
            return View(events);
        }

        // GET: /Organiser/Create
        [HttpGet]
        public IActionResult Create()
        {
            var check = RequireOrganiser();
            if (check != null) return check;
            return View(new CreateEventRequest { EventDate = DateTime.Today });
        }

        // POST: /Organiser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventRequest model)
        {
            var check = RequireOrganiser();
            if (check != null) return check;

            if (!ModelState.IsValid) return View(model);

            var response = await _api.SendAsync(HttpMethod.Post, "api/events", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Could not create event. Please check your details and try again.");
                return View(model);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Organiser/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var check = RequireOrganiser();
            if (check != null) return check;

            var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var evt = await _api.ReadAsync<EventDto>(response);
            if (evt == null) return NotFound();

            var model = new UpdateEventRequest
            {
                Name = evt.Name,
                Description = evt.Description,
                EventDate = evt.EventDate,
                Location = evt.Location,
                DistanceKm = evt.DistanceKm,
                EventType = evt.EventType
            };

            ViewBag.EventId = evt.EventId;
            return View(model);
        }

        // POST: /Organiser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEventRequest model)
        {
            var check = RequireOrganiser();
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = id;
                return View(model);
            }

            var response = await _api.SendAsync(HttpMethod.Put, $"api/events/{id}", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Could not update event.");
                ViewBag.EventId = id;
                return View(model);
            }

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction("Index");
        }

        // POST: /Organiser/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var check = RequireOrganiser();
            if (check != null) return check;

            var response = await _api.SendAsync(HttpMethod.Delete, $"api/events/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Event deleted.";
            else
                TempData["Error"] = "Could not delete event.";

            return RedirectToAction("Index");
        }

        // GET: /Organiser/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var check = RequireOrganiser();
            if (check != null) return check;

            var eventResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            if (!eventResponse.IsSuccessStatusCode) return NotFound();
            var evt = await _api.ReadAsync<EventDto>(eventResponse);

            var enrolResponse = await _api.SendAsync(HttpMethod.Get, $"api/enrolments/event/{id}");
            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(enrolResponse) ?? new List<EnrolmentDto>();

            ViewBag.Event = evt;
            return View(enrolments);
        }
    }
}