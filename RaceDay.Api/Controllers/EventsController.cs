


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ---------- Helper: map Event -> EventDto ----------
        private static EventDto ToDto(Event e)
        {
            return new EventDto
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                EventType = e.EventType,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser != null
                    ? $"{e.Organiser.FirstName} {e.Organiser.LastName}"
                    : string.Empty
            };
        }

        // =========================================================
        // GET /api/events  —  Both roles can view all events
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Must be logged in
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            var events = await _context.Events
                .Include(e => e.Organiser)
                .Select(e => ToDto(e))
                .ToListAsync();

            return Ok(events);
        }

        // =========================================================
        // GET /api/events/mine  —  Organiser only (their own events)
        // =========================================================
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can view their events." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var events = await _context.Events
                .Where(e => e.OrganiserId == organiserId)
                .Include(e => e.Organiser)
                .Select(e => ToDto(e))
                .ToListAsync();

            return Ok(events);
        }

        // =========================================================
        // GET /api/events/{id}  —  Both roles can view one event
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            var evt = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null)
                return NotFound(new { message = "Event not found." });

            return Ok(ToDto(evt));
        }

        // =========================================================
        // POST /api/events  —  Organiser only
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can create events." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var evt = new Event
            {
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                OrganiserId = organiserId
            };

            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            // Reload with Organiser included for the DTO
            await _context.Entry(evt).Reference(e => e.Organiser).LoadAsync();

            return Ok(ToDto(evt));
        }

        // =========================================================
        // PUT /api/events/{id}  —  Organiser who owns the event
        // =========================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventRequest request)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can update events." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var evt = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null)
                return NotFound(new { message = "Event not found." });

            // Ownership check
            if (evt.OrganiserId != organiserId)
                return StatusCode(403, new { message = "You can only update your own events." });

            evt.Name = request.Name;
            evt.Description = request.Description;
            evt.EventDate = request.EventDate;
            evt.Location = request.Location;
            evt.DistanceKm = request.DistanceKm;
            evt.EventType = request.EventType;

            await _context.SaveChangesAsync();

            return Ok(ToDto(evt));
        }

        // =========================================================
        // DELETE /api/events/{id}  —  Organiser who owns the event
        // =========================================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can delete events." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var evt = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null)
                return NotFound(new { message = "Event not found." });

            if (evt.OrganiserId != organiserId)
                return StatusCode(403, new { message = "You can only delete your own events." });

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }
}
