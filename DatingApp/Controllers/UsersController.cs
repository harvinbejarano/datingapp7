namespace DatingApp.Controllers
{
    using AutoMapper;
    using DatingApp.Dtos;
    using DatingApp.Entities;
    using DatingApp.Extensions;
    using DatingApp.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(
            IUserRepository userRepository, 
            IMapper mapper, 
            IPhotoService photoService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            return Ok(await this.userRepository.GetMembersAsync());
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return Ok(await this.userRepository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return NotFound();

            mapper.Map(memberUpdateDto, user);

            if( await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUsernameAsync( User.GetUsername());
            if (user == null) return NotFound();

            var uploadResult = await photoService.AddPhotoAsync(file);
            if(uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

            var photo = new Photo
            {
                Url = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
            };

            if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await userRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUser), 
                        new {username = user.UserName}, mapper.Map<PhotoDto>(photo)); 
            }

            return BadRequest("Problem adding photo");
        }
    }
}
