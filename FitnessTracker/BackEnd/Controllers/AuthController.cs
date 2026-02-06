using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FitnessTracker.BackEnd.Data;
using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FitnessTracker.BackEnd.Controllers
{
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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var email = (request.Email ?? "").Trim().ToLower();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required.");

            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return BadRequest("Email is already registered.");

            CreatePasswordHash(request.Password, out var hash, out var salt);

            var user = new AppUser
            {
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var email = (request.Email ?? "").Trim().ToLower();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            var valid = VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!valid)
                return Unauthorized("Invalid credentials.");

            var token = CreateJwtToken(user);

            return Ok(new AuthResponse { Token = token });
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
