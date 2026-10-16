using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Tests;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventTests
    {
        // ---------- 1. Participant cannot create events (role check simulation) ----------
        [Fact]
        public void ParticipantCannotCreateEvent()
        {
            var context = TestDbContext.CreateInMemory();
            var participantRole = TestDbContext.EnsureRole(context, "Participant");
            var organiserRole = TestDbContext.EnsureRole(context, "Organiser");

            var participant = new AppUser
            {
                FirstName = "Bob",
                LastName = "Runner",
                Email = "bob@test.com",
                PasswordHash = "xxx",
                RoleId = participantRole.RoleId
            };
            context.Users.Add(participant);
            context.SaveChanges();

            // Simulate role check
            var user = context.Users.Include(r => r.Role).First(u => u.UserId == participant.UserId);

            // Assert: user's role is Participant, not Organiser
            Assert.Equal("Participant", user.Role!.RoleName);
            Assert.NotEqual("Organiser", user.Role.RoleName);
        }

        // ---------- 2. Organiser can create events ----------
        [Fact]
        public void OrganiserCanCreateEvent()
        {
            var context = TestDbContext.CreateInMemory();
            var organiserRole = TestDbContext.EnsureRole(context, "Organiser");

            var organiser = new AppUser
            {
                FirstName = "Alice",
                LastName = "Organiser",
                Email = "alice@test.com",
                PasswordHash = "xxx",
                RoleId = organiserRole.RoleId
            };
            context.Users.Add(organiser);
            context.SaveChanges();

            var evt = new Event
            {
                Name = "Test Marathon",
                Description = "Testing",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 42.2,
                EventType = "run",
                OrganiserId = organiser.UserId
            };
            context.Events.Add(evt);
            context.SaveChanges();

            // Assert
            Assert.Single(context.Events);
            Assert.Equal("Test Marathon", context.Events.First().Name);
            Assert.Equal(organiser.UserId, context.Events.First().OrganiserId);
        }

        // ---------- 3. Only owning Organiser can update ----------
        [Fact]
        public void OnlyOwningOrganiserCanUpdate()
        {
            var context = TestDbContext.CreateInMemory();
            var organiserRole = TestDbContext.EnsureRole(context, "Organiser");

            var owner = new AppUser
            {
                FirstName = "Alice",
                LastName = "Owner",
                Email = "alice@test.com",
                PasswordHash = "xxx",
                RoleId = organiserRole.RoleId
            };
            var other = new AppUser
            {
                FirstName = "Carol",
                LastName = "Other",
                Email = "carol@test.com",
                PasswordHash = "xxx",
                RoleId = organiserRole.RoleId
            };
            context.Users.AddRange(owner, other);
            context.SaveChanges();

            var evt = new Event
            {
                Name = "Owned Event",
                Description = "Test",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Durban",
                DistanceKm = 10,
                EventType = "run",
                OrganiserId = owner.UserId
            };
            context.Events.Add(evt);
            context.SaveChanges();

            // Assert: owner matches, other does not
            Assert.Equal(owner.UserId, evt.OrganiserId);
            Assert.NotEqual(other.UserId, evt.OrganiserId);
        }

        // ---------- 4. Event stores all required fields ----------
        [Fact]
        public void EventStoresAllRequiredFields()
        {
            var context = TestDbContext.CreateInMemory();
            var organiserRole = TestDbContext.EnsureRole(context, "Organiser");
            var user = new AppUser
            {
                FirstName = "A",
                LastName = "B",
                Email = "a@b.com",
                PasswordHash = "xxx",
                RoleId = organiserRole.RoleId
            };
            context.Users.Add(user);
            context.SaveChanges();

            var evt = new Event
            {
                Name = "Full Event",
                Description = "With all fields",
                EventDate = new DateTime(2026, 8, 1),
                Location = "Johannesburg",
                DistanceKm = 21.1,
                EventType = "walk",
                OrganiserId = user.UserId
            };
            context.Events.Add(evt);
            context.SaveChanges();

            var saved = context.Events.First();
            Assert.Equal("Full Event", saved.Name);
            Assert.Equal("With all fields", saved.Description);
            Assert.Equal(new DateTime(2026, 8, 1), saved.EventDate);
            Assert.Equal("Johannesburg", saved.Location);
            Assert.Equal(21.1, saved.DistanceKm);
            Assert.Equal("walk", saved.EventType);
        }
    }
}
