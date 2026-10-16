

using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests
{
    public static class TestDbContext
    {
        public static RaceDayDbContext CreateInMemory()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public static Role EnsureRole(RaceDayDbContext context, string roleName)
        {
            var role = context.Roles.FirstOrDefault(r => r.RoleName == roleName);
            if (role == null)
            {
                role = new Role { RoleName = roleName };
                context.Roles.Add(role);
                context.SaveChanges();
            }
            return role;
        }
    }
}



