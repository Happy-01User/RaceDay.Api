using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Api.Data;
using RaceDay.Contracts; // Ensure your DTOs are in this namespace

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events);
        }

        // GET: api/events/mine
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyEvents()
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");
            if (organiserId == null) return Unauthorized();

            var events = await _context.Events.Where(e => e.OrganiserId == organiserId).ToListAsync();
            return Ok(events);
        }

        // POST: api/events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId");
            if (organiserId == null) return Unauthorized();

            var newEvent = new Event
            {
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                OrganiserId = organiserId.Value
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(newEvent); // Return the created event so MVC gets the EventId
        }

        // POST: api/events/{id}/banner
        [HttpPost("{id}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file, [FromServices] BlobStorageService blobStorage)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            string url = await blobStorage.UploadAsync(file, "event-banners");
            eventItem.BannerImageUrl = url;
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl = url });
        }
    }
}
