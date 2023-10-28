using AutoMapper;
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
        private readonly IMapper mapper;

        public AccountController(
            DataContext context, 
            ITokenService tokenService,
            IMapper mapper)
        {
            this.context = context;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto userDto)
        {
            if( await UserExists(userDto.Username)) return BadRequest("User name is taken!");

            var user = mapper.Map<AppUser>(userDto);

            using var hmac = new HMACSHA512();

            user.UserName = userDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password));
            user.PasswordSalt = hmac.Key;


            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            return new UserDto 
            {
                Username = user.UserName,
                Token = tokenService.CreateToken (user),
                KnownAs = user.KnownAs
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await this.context.Users
                .Include(x => x.Photos)
                .FirstOrDefaultAsync( u => u.UserName == loginDto.Username);
            
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
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(f => f.IsMain)?.Url,
                KnownAs = user.KnownAs
            };

        }

        private async Task<bool> UserExists(string username)
        {
            return await this.context.Users.AnyAsync(u => u.UserName == username.ToLower());
        }

    }
}
