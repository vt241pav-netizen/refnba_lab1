using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NBA.EFCore.Data;
using NBA.EFCore.DTOs;
using NBA.EFCore.EFModels;

namespace NBA.EFCore.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(LoginDto loginDto);
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task InitializeTestUsersAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly NbaDbContext _context;

        public AuthService(NbaDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null) return null;


            if (user.PasswordHash == loginDto.Password)
            {
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return user;
            }
            
            var inputHash = HashPasswordSHA256(loginDto.Password);
            if (user.PasswordHash == inputHash)
            {
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return user;
            }
            

            return null;
        }

        public Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task InitializeTestUsersAsync()
        {
            var users = new[]
            {
                new User
                {
                    Username = "dev_user",
                    PasswordHash = "123", 
                    UserRole = "Developer",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "analyst_user",
                    PasswordHash = "123",
                    UserRole = "Analyst",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "admin_user",
                    PasswordHash = "123",
                    UserRole = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var user in users)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username);
                
                if (existingUser == null)
                {
                    await _context.Users.AddAsync(user);
                }
                else
                {
                    existingUser.PasswordHash = user.PasswordHash;
                    existingUser.UserRole = user.UserRole;
                    existingUser.IsActive = user.IsActive;
                }
            }
            
            await _context.SaveChangesAsync();
        }

        public Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            throw new NotImplementedException();
        }

        private string HashPasswordSHA256(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}