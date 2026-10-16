


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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ---------- Helper: map Enrolment -> EnrolmentDto ----------
        private static EnrolmentDto ToDto(Enrolment en)
        {
            return new EnrolmentDto
            {
                EnrolmentId = en.EnrolmentId,
                EventId = en.EventId,
                EventName = en.Event?.Name ?? string.Empty,
                CategoryId = en.CategoryId,
                CategoryName = en.Category?.Name ?? string.Empty,
                ParticipantId = en.ParticipantId,
                ParticipantName = en.Participant != null
                    ? $"{en.Participant.FirstName} {en.Participant.LastName}"
                    : string.Empty,
                Status = en.Status,
                EnrolledOn = en.EnrolledOn
            };
        }

        // =========================================================
        // POST /api/enrolments  —  Participant only
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentRequest request)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants can enrol in events." });

            var participantId = SessionHelper.GetUserId(HttpContext)!.Value;

            // Confirm event exists
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == request.EventId);
            if (evt == null)
                return NotFound(new { message = "Event not found." });

            // Confirm category exists AND belongs to that event
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.EventId != request.EventId)
                return BadRequest(new { message = "Category does not belong to the specified event." });

            // Prevent duplicate enrolment for the same event by the same participant
            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(en => en.ParticipantId == participantId && en.EventId == request.EventId);
            if (alreadyEnrolled)
                return BadRequest(new { message = "You are already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                Status = "Pending",
                EnrolledOn = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            // Reload with nav props for the DTO
            var created = await _context.Enrolments
                .Include(en => en.Event)
                .Include(en => en.Category)
                .Include(en => en.Participant)
                .FirstAsync(en => en.EnrolmentId == enrolment.EnrolmentId);

            return Ok(ToDto(created));
        }

        // =========================================================
        // GET /api/enrolments/mine  —  Participant only
        // =========================================================
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Participant"))
                return StatusCode(403, new { message = "Only Participants have their own enrolments." });

            var participantId = SessionHelper.GetUserId(HttpContext)!.Value;

            var enrolments = await _context.Enrolments
                .Where(en => en.ParticipantId == participantId)
                .Include(en => en.Event)
                .Include(en => en.Category)
                .Include(en => en.Participant)
                .Select(en => ToDto(en))
                .ToListAsync();

            return Ok(enrolments);
        }

        // =========================================================
        // GET /api/enrolments/event/{eventId}  —  Owning Organiser
        // =========================================================
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can view event enrolments." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null)
                return NotFound(new { message = "Event not found." });

            if (evt.OrganiserId != organiserId)
                return StatusCode(403, new { message = "You can only view enrolments for your own events." });

            var enrolments = await _context.Enrolments
                .Where(en => en.EventId == eventId)
                .Include(en => en.Event)
                .Include(en => en.Category)
                .Include(en => en.Participant)
                .Select(en => ToDto(en))
                .ToListAsync();

            return Ok(enrolments);
        }

        // =========================================================
        // PUT /api/enrolments/{id}/status  —  Owning Organiser
        // =========================================================
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusRequest request)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can update enrolment statuses." });

            // Validate status
            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Status must be 'Pending', 'Confirmed' or 'Cancelled'." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var enrolment = await _context.Enrolments
                .Include(en => en.Event)
                .Include(en => en.Category)
                .Include(en => en.Participant)
                .FirstOrDefaultAsync(en => en.EnrolmentId == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            // Ownership check via the parent event
            if (enrolment.Event == null || enrolment.Event.OrganiserId != organiserId)
                return StatusCode(403, new { message = "You can only manage enrolments for your own events." });

            enrolment.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(ToDto(enrolment));
        }
    }
}
