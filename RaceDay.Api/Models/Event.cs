using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;

        public int OrganiserId { get; set; }
        public string? BannerImageUrl { get; set; }

        // ---------- Navigation properties - THIS WAS MISSING ----------
        public AppUser Organiser { get; set; } = null!;
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
