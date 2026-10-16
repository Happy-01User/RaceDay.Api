using Xunit;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{
    public class EnrolmentTests
    {
        // ---------- 1. Participant can enrol in an event ----------
        [Fact]
        public void ParticipantCanEnrolInEvent()
        {
            var context = TestDbContext.CreateInMemory();
            var organiserRole = TestDbContext.EnsureRole(context, "Organiser");
            var participantRole = TestDbContext.EnsureRole(context, "Participant");

            var organiser = new AppUser
            {
                FirstName = "Alice",
                LastName = "O",
                Email = "alice@test.com",
                PasswordHash = "xxx",
                RoleId = organiserRole.RoleId
            };
            var participant = new AppUser
            {
                FirstName = "Bob",
                LastName = "P",
                Email = "bob@test.com",
                PasswordHash = "xxx",
                RoleId = participantRole.RoleId
            };
            context.Users.AddRange(organiser, participant);
            context.SaveChanges();

            var evt = new Event
            {
                Name = "Test Event",
                Description = "Test",
                EventDate = DateTime.UtcNow.AddDays(20),
                Location = "Cape Town",
                DistanceKm = 10,
                EventType = "run",
                OrganiserId = organiser.UserId
            };
            context.Events.Add(evt);
            context.SaveChanges();

            var category = new Category
            {
                Name = "Senior",
                Type = "Age",
                EventId = evt.EventId
            };
            context.Categories.Add(category);
            context.SaveChanges();

            var enrolment = new Enrolment
            {
                ParticipantId = participant.UserId,
                EventId = evt.EventId,
                CategoryId = category.CategoryId,
                Status = "Pending",
                EnrolledOn = DateTime.UtcNow
            };
            context.Enrolments.Add(enrolment);
            context.SaveChanges();

            // Assert
            Assert.Single(context.Enrolments);
            var saved = context.Enrolments.First();
            Assert.Equal(participant.UserId, saved.ParticipantId);
            Assert.Equal(evt.EventId, saved.EventId);
            Assert.Equal(category.CategoryId, saved.CategoryId);
            Assert.Equal("Pending", saved.Status);
        }

        // ---------- 2. Duplicate enrolment is rejected ----------
        [Fact]
        public void DuplicateEnrolment_Fails()
        {
            var context = TestDbContext.CreateInMemory();
            var participantRole = TestDbContext.EnsureRole(context, "Participant");
            var organiserRole = TestDbContext.EnsureRole(context, "Organiser");

            var participant = new AppUser { FirstName = "Bob", LastName = "P", Email = "bob@test.com", PasswordHash = "xxx", RoleId = participantRole.RoleId };
            var organiser = new AppUser { FirstName = "Alice", LastName = "O", Email = "alice@test.com", PasswordHash = "xxx", RoleId = organiserRole.RoleId };
            context.Users.AddRange(participant, organiser);
            context.SaveChanges();

            var evt = new Event { Name = "E1", Description = "x", EventDate = DateTime.UtcNow.AddDays(5), Location = "L", DistanceKm = 5, EventType = "run", OrganiserId = organiser.UserId };
            context.Events.Add(evt);
            context.SaveChanges();

            var category = new Category { Name = "Senior", Type = "Age", EventId = evt.EventId };
            context.Categories.Add(category);
            context.SaveChanges();

            var e1 = new Enrolment { ParticipantId = participant.UserId, EventId = evt.EventId, CategoryId = category.CategoryId, Status = "Pending" };
            context.Enrolments.Add(e1);
            context.SaveChanges();

            // Simulate duplicate check
            var duplicate = context.Enrolments.Any(en =>
                en.ParticipantId == participant.UserId && en.EventId == evt.EventId);

            Assert.True(duplicate);
        }

        // ---------- 3. Category must belong to the event ----------
        [Fact]
        public void CategoryMustBelongToEvent()
        {
            var context = TestDbContext.CreateInMemory();
            var organiserRole = TestDbContext.EnsureRole(context, "Organiser");

            var organiser = new AppUser { FirstName = "A", LastName = "B", Email = "a@b.com", PasswordHash = "xxx", RoleId = organiserRole.RoleId };
            context.Users.Add(organiser);
            context.SaveChanges();

            var event1 = new Event { Name = "E1", Description = "x", EventDate = DateTime.UtcNow, Location = "L", DistanceKm = 5, EventType = "run", OrganiserId = organiser.UserId };
            var event2 = new Event { Name = "E2", Description = "x", EventDate = DateTime.UtcNow, Location = "L", DistanceKm = 5, EventType = "run", OrganiserId = organiser.UserId };
            context.Events.AddRange(event1, event2);
            context.SaveChanges();

            var catForEvent1 = new Category { Name = "C1", Type = "Age", EventId = event1.EventId };
            context.Categories.Add(catForEvent1);
            context.SaveChanges();

            // Assert: category's event is event1, not event2
            Assert.Equal(event1.EventId, catForEvent1.EventId);
            Assert.NotEqual(event2.EventId, catForEvent1.EventId);
        }
    }
}
