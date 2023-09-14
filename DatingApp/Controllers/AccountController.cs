using DatingApp.Data;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto userDto)
        {
            if( await UserExists(userDto.Username)) return BadRequest("User name is taken!");

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = userDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password)),
                PasswordSalt = hmac.Key
            };

            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            return new UserDto 
            {
                Username = userDto.Username,
                Token = tokenService.CreateToken (user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await this.context.Users.FirstOrDefaultAsync( u => u.UserName == loginDto.Username);
            
            if(user == null) return Unauthorized();

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for(var i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized();
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user)
            };

        }

        private async Task<bool> UserExists(string username)
        {
            return await this.context.Users.AnyAsync(u => u.UserName == username.ToLower());
        }

    }
}
