



using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ---------- Helper: map Category -> CategoryDto ----------
        private static CategoryDto ToDto(Category c)
        {
            return new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Type = c.Type,
                EventId = c.EventId
            };
        }

        // =========================================================
        // GET /api/events/{eventId}/categories  —  Both roles
        // =========================================================
        [HttpGet("events/{eventId:int}/categories")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            // Confirm event exists
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => ToDto(c))
                .ToListAsync();

            return Ok(categories);
        }

        // =========================================================
        // POST /api/events/{eventId}/categories  —  Owning Organiser
        // =========================================================
        [HttpPost("events/{eventId:int}/categories")]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryRequest request)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can create categories." });

            // Validate Type
            if (request.Type != "Age" && request.Type != "Distance")
                return BadRequest(new { message = "Type must be 'Age' or 'Distance'." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null)
                return NotFound(new { message = "Event not found." });

            // Ownership check
            if (evt.OrganiserId != organiserId)
                return StatusCode(403, new { message = "You can only add categories to your own events." });

            var category = new Category
            {
                Name = request.Name,
                Type = request.Type,
                EventId = eventId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(ToDto(category));
        }

        // =========================================================
        // PUT /api/categories/{id}  —  Owning Organiser
        // =========================================================
        [HttpPut("categories/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryRequest request)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can update categories." });

            if (request.Type != "Age" && request.Type != "Distance")
                return BadRequest(new { message = "Type must be 'Age' or 'Distance'." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            // Ownership check through the parent event
            if (category.Event == null || category.Event.OrganiserId != organiserId)
                return StatusCode(403, new { message = "You can only update categories on your own events." });

            category.Name = request.Name;
            category.Type = request.Type;

            await _context.SaveChangesAsync();

            return Ok(ToDto(category));
        }

        // =========================================================
        // DELETE /api/categories/{id}  —  Owning Organiser
        // =========================================================
        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in first." });

            if (!SessionHelper.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Only Organisers can delete categories." });

            var organiserId = SessionHelper.GetUserId(HttpContext)!.Value;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event == null || category.Event.OrganiserId != organiserId)
                return StatusCode(403, new { message = "You can only delete categories on your own events." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}
