using ApiContracts.Comments;
using ApiContracts.Posts;
using Entities;
using Microsoft.AspNetCore.Mvc;
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

        if (!UserExists(request.UserId))
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
        PostDto dto = ToDto(created);
        return Created($"/Posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDto>> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        Post existing;
        try
        {
            existing = await postRepo.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Post with id {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body cannot be empty.");
        }

        existing.Title = request.Title;
        existing.Body = request.Body;

        await postRepo.UpdateAsync(existing);
        return Ok(ToDto(existing));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetSinglePost([FromRoute] int id, [FromQuery] bool includeComments = false)
    {
        Post post;
        try
        {
            post = await postRepo.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Post with id {id} not found.");
        }

        if (!includeComments)
        {
            return Ok(ToDto(post));
        }

        PostWithCommentsDto dto = new PostWithCommentsDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            Username = UsernameFor(post.UserId),
            Comments = commentRepo.GetManyAsync()
                .Where(c => c.PostId == post.Id)
                .OrderBy(c => c.Id)
                .Select(CommentToDto)
                .ToList()
        };

        return Ok(dto);
    }

    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> GetPosts(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? username)
    {
        IQueryable<Post> posts = postRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            posts = posts.Where(p => p.Title.ToLower().Contains(titleContains.ToLower()));
        }

        if (userId.HasValue)
        {
            posts = posts.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            List<int> matchingUserIds = userRepo.GetManyAsync()
                .Where(u => u.Username.ToLower().Contains(username.ToLower()))
                .Select(u => u.Id)
                .ToList();

            posts = posts.Where(p => matchingUserIds.Contains(p.UserId));
        }

        List<PostDto> result = posts.OrderBy(p => p.Id).Select(ToDto).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}/comments")]
    public ActionResult<IEnumerable<CommentDto>> GetCommentsForPost([FromRoute] int id)
    {
        if (!postRepo.GetManyAsync().Any(p => p.Id == id))
        {
            return NotFound($"Post with id {id} not found.");
        }

        List<CommentDto> result = commentRepo.GetManyAsync()
            .Where(c => c.PostId == id)
            .OrderBy(c => c.Id)
            .Select(CommentToDto)
            .ToList();

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePost([FromRoute] int id)
    {
        try
        {
            await postRepo.DeleteAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Post with id {id} not found.");
        }

        List<int> commentIds = commentRepo.GetManyAsync()
            .Where(c => c.PostId == id)
            .Select(c => c.Id)
            .ToList();

        foreach (int commentId in commentIds)
        {
            await commentRepo.DeleteAsync(commentId);
        }

        return NoContent();
    }

    private bool UserExists(int userId)
    {
        return userRepo.GetManyAsync().Any(u => u.Id == userId);
    }

    private string UsernameFor(int userId)
    {
        User? user = userRepo.GetManyAsync().SingleOrDefault(u => u.Id == userId);
        return user is null ? string.Empty : user.Username;
    }

    private PostDto ToDto(Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            Username = UsernameFor(post.UserId)
        };
    }

    private CommentDto CommentToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Username = UsernameFor(comment.UserId)
        };
    }
}
