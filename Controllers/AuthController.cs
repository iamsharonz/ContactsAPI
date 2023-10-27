using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ContactsAPI.Data;
using ContactsAPI.Dtos;
using ContactsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ContactsAPI.Controllers
{
    
    // [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        public readonly ContactsAPIDbContext _context;
        
        public readonly IConfiguration _configuration;
        public AuthController(ContactsAPIDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserDto request)
        {
            if(await UserExists(request.Username))
            {
                return BadRequest("User already exists!!");
            }
            CreatePasswordHash(request.Password, out byte[] PasswordHash, out byte[] PasswordSalt);

            user.Username = request.Username;
            user.PasswordHash = PasswordHash;
            user.PasswordSalt = PasswordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            return Ok(user);

        }

        public async Task<bool> UserExists(string Username)
        {
            if(await _context.Users.AnyAsync(u => u.Username.ToLower().Equals(Username)))
            {
                return true;
            }
            return false;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(request.Username));
            if (user?.Username != request.Username)
            {
                return BadRequest("User Not FOund!!");

            }

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong Password");
            }

            var token = CreateToken(user);

            return Ok(token);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var Key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));
            
            var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public void CreatePasswordHash(string Password, out byte[] PasswordHash, out byte[] PasswordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                PasswordSalt = hmac.Key;
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password));
            }
        }
        public bool VerifyPassword(string Password, byte[] PasswordHash, byte[] PasswordSalt)
        {
            using (var hmac = new HMACSHA512(PasswordSalt))
            {
                var ComputedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password));
                return ComputedHash.SequenceEqual(PasswordHash);
            }
        }
    }
}