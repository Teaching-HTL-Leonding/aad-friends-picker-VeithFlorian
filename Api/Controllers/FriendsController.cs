using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendsController : ControllerBase
    { 
        private readonly UserContext context;

        public FriendsController(UserContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [Route("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        public IActionResult GetAllFriends()
        {
            string userId = User.Claims.First(claim => claim.Type == ClaimConstants.ObjectId).Value;
            return Ok(context.Users.Where(u => u.UserId == userId).Select(u => u.FriendId));
        }
        
        [HttpGet]
        [Route("getById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public IActionResult GetById([FromQuery(Name = "friendId")] string friendId) 
        {
            string userId = User.Claims.First(claim => claim.Type == ClaimConstants.ObjectId).Value;
            return Ok(context.Users.First(u => u.UserId == userId && u.FriendId == friendId).FriendId);
        }

        [HttpPost]
        [Route("add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddFriend([FromQuery(Name = "friendId")] string friendId)
        {            
            string userId = User.Claims.First(claim => claim.Type == ClaimConstants.ObjectId).Value;
            if (context.Users.Any(u => u.UserId == userId && u.FriendId == friendId))
            {
                return BadRequest("Friend already exists");
            }
            
            await context.Users.AddAsync(new Data.User() { UserId = userId, FriendId = friendId });
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { UserId = userId, FriendId = friendId }, new { UserId = userId, FriendId = friendId });
        }
        
        [HttpDelete]
        [Route("remove")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveFriend([FromQuery(Name = "friendId")] string friendId)
        {
            string userId = User.Claims.First(claim => claim.Type == ClaimConstants.ObjectId).Value;
            if (!context.Users.Any(u => u.UserId == userId && u.FriendId == friendId))
            {
                return NotFound();
            }

            context.Users.Remove(context.Users.First(u => u.UserId == userId && u.FriendId == friendId));
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}