using ApiContracts.Comments;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;

    public CommentsController(ICommentRepository commentRepo, IPostRepository postRepo, IUserRepository userRepo)
    {
        this.commentRepo = commentRepo;
        this.postRepo = postRepo;
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body cannot be empty.");
        }

        if (!postRepo.GetManyAsync().Any(p => p.Id == request.PostId))
        {
            return BadRequest($"No post with id {request.PostId}.");
        }

        if (!userRepo.GetManyAsync().Any(u => u.Id == request.UserId))
        {
            return BadRequest($"No user with id {request.UserId}.");
        }

        Comment comment = new Comment
        {
            Body = request.Body,
            PostId = request.PostId,
            UserId = request.UserId
        };

        Comment created = await commentRepo.AddAsync(comment);
        CommentDto dto = ToDto(created);
        return Created($"/Comments/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDto>> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        Comment existing;
        try
        {
            existing = await commentRepo.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Comment with id {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body cannot be empty.");
        }

        existing.Body = request.Body;

        await commentRepo.UpdateAsync(existing);
        return Ok(ToDto(existing));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetSingleComment([FromRoute] int id)
    {
        try
        {
            Comment comment = await commentRepo.GetSingleAsync(id);
            return Ok(ToDto(comment));
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Comment with id {id} not found.");
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? username,
        [FromQuery] int? postId)
    {
        IQueryable<Comment> comments = commentRepo.GetManyAsync();

        if (userId.HasValue)
        {
            comments = comments.Where(c => c.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            List<int> matchingUserIds = userRepo.GetManyAsync()
                .Where(u => u.Username.ToLower().Contains(username.ToLower()))
                .Select(u => u.Id)
                .ToList();

            comments = comments.Where(c => matchingUserIds.Contains(c.UserId));
        }

        if (postId.HasValue)
        {
            comments = comments.Where(c => c.PostId == postId.Value);
        }

        List<CommentDto> result = comments.OrderBy(c => c.Id).Select(ToDto).ToList();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment([FromRoute] int id)
    {
        try
        {
            await commentRepo.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Comment with id {id} not found.");
        }
    }

    private CommentDto ToDto(Comment comment)
    {
        User? user = userRepo.GetManyAsync().SingleOrDefault(u => u.Id == comment.UserId);
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Username = user is null ? string.Empty : user.Username
        };
    }
}
