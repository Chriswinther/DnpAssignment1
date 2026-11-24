using ApiContracts.Comments;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        bool postExists = await postRepo.GetManyAsync().AnyAsync(p => p.Id == request.PostId);
        if (!postExists)
        {
            return BadRequest($"No post with id {request.PostId}.");
        }

        User? author = await userRepo.GetManyAsync()
            .SingleOrDefaultAsync(u => u.Id == request.UserId);
        if (author is null)
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

        CommentDto dto = new CommentDto
        {
            Id = created.Id,
            Body = created.Body,
            PostId = created.PostId,
            UserId = created.UserId,
            Username = author.Username
        };

        return Created($"/Comments/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDto>> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body cannot be empty.");
        }

        Comment existing;
        try
        {
            existing = await commentRepo.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Comment with id {id} not found.");
        }

        existing.Body = request.Body;
        await commentRepo.UpdateAsync(existing);

        CommentDto? dto = await commentRepo.GetManyAsync()
            .Where(c => c.Id == id)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username
            })
            .FirstOrDefaultAsync();

        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetSingleComment([FromRoute] int id)
    {
        CommentDto? dto = await commentRepo.GetManyAsync()
            .Where(c => c.Id == id)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username
            })
            .FirstOrDefaultAsync();

        return dto is null ? NotFound($"Comment with id {id} not found.") : Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? username,
        [FromQuery] int? postId)
    {
        IQueryable<Comment> query = commentRepo.GetManyAsync();

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            string filter = username.ToLower();
            query = query.Where(c => c.User.Username.ToLower().Contains(filter));
        }

        if (postId.HasValue)
        {
            query = query.Where(c => c.PostId == postId.Value);
        }

        List<CommentDto> result = await query
            .OrderBy(c => c.Id)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username
            })
            .ToListAsync();

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
}
