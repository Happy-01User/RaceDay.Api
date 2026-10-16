using Xunit;
using RaceDay.Api.Services;
using RaceDay.Contracts;

namespace RaceDay.Api.Tests
{
    public class AuthTests
    {
        // ---------- 1. Valid registration succeeds ----------
        [Fact]
        public async Task ValidRegistration_Succeeds()
        {
            // Arrange
            var context = TestDbContext.CreateInMemory();
            var service = new AuthService(context);

            var request = new RegisterRequest
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com",
                Password = "Password123!",
                Role = "Organiser"
            };

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("alice@test.com", result!.Email);
            Assert.Equal("Organiser", result.Role);
        }

        // ---------- 2. Duplicate email registration fails ----------
        [Fact]
        public async Task DuplicateEmailRegistration_Fails()
        {
            var context = TestDbContext.CreateInMemory();
            var service = new AuthService(context);

            var request = new RegisterRequest
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com",
                Password = "Password123!",
                Role = "Organiser"
            };

            await service.RegisterAsync(request);

            // Try to register again with the same email
            await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(request));
        }

        // ---------- 3. Invalid role is rejected ----------
        [Fact]
        public async Task InvalidRole_Fails()
        {
            var context = TestDbContext.CreateInMemory();
            var service = new AuthService(context);

            var request = new RegisterRequest
            {
                FirstName = "Bob",
                LastName = "Jones",
                Email = "bob@test.com",
                Password = "Password123!",
                Role = "Admin" // invalid
            };

            await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(request));
        }

        // ---------- 4. Valid login succeeds ----------
        [Fact]
        public async Task ValidLogin_Succeeds()
        {
            var context = TestDbContext.CreateInMemory();
            var service = new AuthService(context);

            await service.RegisterAsync(new RegisterRequest
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com",
                Password = "Password123!",
                Role = "Organiser"
            });

            var result = await service.LoginAsync(new LoginRequest
            {
                Email = "alice@test.com",
                Password = "Password123!"
            });

            Assert.NotNull(result);
            Assert.Equal("alice@test.com", result!.Email);
        }

        // ---------- 5. Incorrect credentials fail ----------
        [Fact]
        public async Task IncorrectPassword_Fails()
        {
            var context = TestDbContext.CreateInMemory();
            var service = new AuthService(context);

            await service.RegisterAsync(new RegisterRequest
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@test.com",
                Password = "Password123!",
                Role = "Organiser"
            });

            var result = await service.LoginAsync(new LoginRequest
            {
                Email = "alice@test.com",
                Password = "WrongPassword!"
            });

            Assert.Null(result);
        }

        // ---------- 6. Login with unknown email fails ----------
        [Fact]
        public async Task UnknownEmailLogin_Fails()
        {
            var context = TestDbContext.CreateInMemory();
            var service = new AuthService(context);

            var result = await service.LoginAsync(new LoginRequest
            {
                Email = "nobody@test.com",
                Password = "Password123!"
            });

            Assert.Null(result);
        }
    }
}

