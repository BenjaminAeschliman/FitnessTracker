using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FitnessTracker.BackEnd.Data;
using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FitnessTracker.BackEnd.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly FitnessDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(FitnessDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var email = (request.Email ?? "").Trim().ToLowerInvariant();
            var password = request.Password ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { error = "Email and password are required." });

            var exists = await _db.Users.AnyAsync(u => u.Email == email, ct);
            if (exists)
                return BadRequest(new { error = "Email is already registered." });

            CreatePasswordHash(password, out var hash, out var salt);

            var user = new AppUser
            {
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            return Ok(new { message = "Registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var email = (request.Email ?? "").Trim().ToLowerInvariant();
            var password = request.Password ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { error = "Email and password are required." });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user == null)
                return Unauthorized(new { error = "Invalid credentials." });

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized(new { error = "Invalid credentials." });

            var token = CreateJwtToken(user);
            return Ok(new { token });
        }

        private string CreateJwtToken(AppUser user)
        {
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(storedHash);
        }
    }
}
