using AutoMapper;
using CommentsApp.Interfaces;
using CommentsApp.Models;
using CommentsApp.Models.DTO;
using CommentsApp.Repository.IRepository;
using CommentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Net;
using System.Security.Claims;

namespace CommentsApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        protected APIResponse _response;
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService) 
        {
            _response = new();
            _commentService = commentService;
        }

        [HttpGet(Name = "GetComments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComments(int pageSize = 25, int pageNumber = 1, string? sortBy = null, bool descending = false) 
        {
            try
            {
                var comments = await _commentService.GetComments(pageSize, pageNumber, sortBy, descending);

                _response.Result = comments;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex) 
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpGet("test-redis")]
        public async Task<IActionResult> TestRedis([FromServices] IConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();
            await db.StringSetAsync("test", "Hello Redis!", TimeSpan.FromMinutes(1));
            var value = await db.StringGetAsync("test");
            return Ok(value);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost(Name = "CreateComment")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateComment([FromForm] CommentCreateDTO commentCreateDTO, IFormFile? file)
        {
            try
            {
                if (commentCreateDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "User is not authenticated" };
                    return BadRequest(_response);
                }

                var commentCreated = await _commentService.CreateComment(commentCreateDTO, userId, file);

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                _response.Result = commentCreated;

                return CreatedAtRoute("GetComments", new { id = commentCreated }, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }  
        }

        [HttpDelete("{id:int}", Name = "DeleteComment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Data client is null" };
                    return BadRequest(_response);
                }

                var isDeleted = await _commentService.DeleteComment(id);

                if (!isDeleted)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Comment not found" };

                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return NoContent();
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        [HttpPut("{id:int}", Name = "UpdateComment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateComment([FromBody]UpdateCommentDTO updateCommentDTO)
        {
            try
            {
                if (updateCommentDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Data client is null" };

                    return BadRequest(_response);
                }

                var isUpdated = await _commentService.UpdateComment(updateCommentDTO);

                if (!isUpdated)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Comment not found or not updated" };
                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return NoContent();
            }
            catch(Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }
    }
}
