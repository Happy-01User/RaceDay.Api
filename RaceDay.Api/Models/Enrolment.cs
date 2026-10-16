

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }

        // Foreign Key to Participant (AppUser)
        public int ParticipantId { get; set; }
        public AppUser? Participant { get; set; }

        // Foreign Key to Event
        public int EventId { get; set; }
        public Event? Event { get; set; }

        // Foreign Key to Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string Status { get; set; } = "Confirmed";
        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Result? Result { get; set; }
    }
}
