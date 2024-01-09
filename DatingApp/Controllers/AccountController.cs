using AutoMapper;
using DatingApp.Data;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(
            UserManager<AppUser> userManager, 
            ITokenService tokenService,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto userDto)
        {
            if( await UserExists(userDto.Username)) return BadRequest("User name is taken!");

            var user = mapper.Map<AppUser>(userDto);

            user.UserName = userDto.Username.ToLower();

            var result = await userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            return new UserDto 
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken (user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManager.Users
                .Include(x => x.Photos)
                .FirstOrDefaultAsync( u => u.UserName == loginDto.Username);
            
            if(user == null) return Unauthorized();

            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result) return Unauthorized();

            return new UserDto
            {
                Username = user.UserName,
                Token = await  tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(f => f.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

        }

        private async Task<bool> UserExists(string username)
        {
            return await userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
        }

    }
}
