

namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; // "Under 20", "Senior", "10km"
        public string Type { get; set; } = string.Empty; // "Age" or "Distance"

        // Foreign Key to Event
        public int EventId { get; set; }
        public Event? Event { get; set; }

        // Navigation property
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}