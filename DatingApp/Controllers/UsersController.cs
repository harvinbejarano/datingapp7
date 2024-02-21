namespace DatingApp.Controllers
{
    using AutoMapper;
    using DatingApp.Dtos;
    using DatingApp.Entities;
    using DatingApp.Extensions;
    using DatingApp.Helpers;
    using DatingApp.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        private readonly IUnitOfWork uow;

        public UsersController(
            IMapper mapper, 
            IPhotoService photoService,
            IUnitOfWork uow)
        {
            this.mapper = mapper;
            this.photoService = photoService;
            this.uow = uow;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await uow.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await uow.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader( users.CurrentPage, 
                                                users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return Ok(await uow.UserRepository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return NotFound();

            mapper.Map(memberUpdateDto, user);

            if( await uow.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await uow.UserRepository.GetUserByUsernameAsync( User.GetUsername());
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

            if (await uow.Complete())
            {
                return CreatedAtAction(nameof(GetUser), 
                        new {username = user.UserName}, mapper.Map<PhotoDto>(photo)); 
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault( f => f.Id == photoId);
            if(photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("Already your main photo");

            var currentMain = user.Photos.FirstOrDefault(f => f.IsMain);
            if(currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if(await uow.Complete()) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(f => f.Id == photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("you cannot delte your main photo.");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);            
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if (await uow.Complete()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}
