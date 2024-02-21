using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork uow;

        public LikesController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await uow.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await uow.LikesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You can not like your self");

            var userlike = await uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userlike != null) return BadRequest("You already like this user");

            var userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await uow.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await uow.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, 
                users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}
