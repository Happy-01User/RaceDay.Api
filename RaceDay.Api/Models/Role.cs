

namespace RaceDay.Api.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty; // "Organiser" or "Participant"

        // Navigation property: one Role has many Users
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}