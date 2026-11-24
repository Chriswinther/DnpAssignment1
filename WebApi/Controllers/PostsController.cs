using ApiContracts.Comments;
using ApiContracts.Posts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo, ICommentRepository commentRepo)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body cannot be empty.");
        }

        User? author = await userRepo.GetManyAsync()
            .SingleOrDefaultAsync(u => u.Id == request.UserId);
        if (author is null)
        {
            return BadRequest($"No user with id {request.UserId}.");
        }

        Post post = new Post
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId
        };

        Post created = await postRepo.AddAsync(post);

        PostDto dto = new PostDto
        {
            Id = created.Id,
            Title = created.Title,
            Body = created.Body,
            UserId = created.UserId,
            Username = author.Username
        };

        return Created($"/Posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDto>> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body cannot be empty.");
        }

        Post existing;
        try
        {
            existing = await postRepo.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Post with id {id} not found.");
        }

        existing.Title = request.Title;
        existing.Body = request.Body;

        await postRepo.UpdateAsync(existing);

        PostDto? dto = await postRepo.GetManyAsync()
            .Where(p => p.Id == id)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                Username = p.User.Username
            })
            .FirstOrDefaultAsync();

        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSinglePost([FromRoute] int id, [FromQuery] bool includeComments = false)
    {
        IQueryable<Post> query = postRepo.GetManyAsync()
            .Where(p => p.Id == id)
            .Include(p => p.User);

        if (includeComments)
        {
            query = query.Include(p => p.Comments).ThenInclude(c => c.User);
        }

        if (!includeComments)
        {
            PostDto? plain = await query
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    UserId = p.UserId,
                    Username = p.User.Username
                })
                .FirstOrDefaultAsync();

            return plain is null ? NotFound($"Post with id {id} not found.") : Ok(plain);
        }

        PostWithCommentsDto? dto = await query
            .Select(p => new PostWithCommentsDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                Username = p.User.Username,
                Comments = p.Comments
                    .OrderBy(c => c.Id)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Body = c.Body,
                        PostId = c.PostId,
                        UserId = c.UserId,
                        Username = c.User.Username
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return dto is null ? NotFound($"Post with id {id} not found.") : Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? username)
    {
        IQueryable<Post> query = postRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            string filter = titleContains.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(filter));
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            string filter = username.ToLower();
            query = query.Where(p => p.User.Username.ToLower().Contains(filter));
        }

        List<PostDto> result = await query
            .OrderBy(p => p.Id)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                Username = p.User.Username
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost([FromRoute] int id)
    {
        bool postExists = await postRepo.GetManyAsync().AnyAsync(p => p.Id == id);
        if (!postExists)
        {
            return NotFound($"Post with id {id} not found.");
        }

        List<CommentDto> result = await commentRepo.GetManyAsync()
            .Where(c => c.PostId == id)
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
    public async Task<IActionResult> DeletePost([FromRoute] int id)
    {
        try
        {
            await postRepo.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Post with id {id} not found.");
        }
    }
}
