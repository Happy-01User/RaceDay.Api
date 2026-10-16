using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Services
{
    public class AuthService
    {
        private readonly RaceDayDbContext _context;

        public AuthService(RaceDayDbContext context)
        {
            _context = context;
        }

        // ---------- REGISTER ----------
        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            // 1. Validate role
            if (request.Role != "Organiser" && request.Role != "Participant")
                throw new Exception("Role must be 'Organiser' or 'Participant'.");

            // 2. Check if email already exists
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                throw new Exception("Email is already registered.");

            // 3. Find or create the role
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == request.Role);
            if (role == null)
            {
                role = new Role { RoleName = request.Role };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
            }

            // 4. Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 5. Create the user
            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash,
                RoleId = role.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 6. Return response
            return new AuthResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = role.RoleName
            };
        }

        // ---------- LOGIN ----------
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            // 1. Find user by email
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return null;

            // 2. Verify password
            var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid)
                return null;

            // 3. Return response
            return new AuthResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role!.RoleName
            };
        }
    }
}
 