using CarSell.DataContext;
using CarSell.Models;
using CarSell.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CarSell.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly CarDbContext _context;

        public AuthenticationController(IConfiguration configuration, ITokenService tokenService, CarDbContext context)
        {
            _configuration = configuration;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists");
            }

            var newUser = new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password)
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == login.Username);

            if (user == null || !VerifyPassword(login.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var accessToken = _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            _context.SaveChanges();

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
