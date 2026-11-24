using ApiContracts.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public UsersController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("Username cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Password cannot be empty.");
        }

        if (await UsernameTakenAsync(request.Username))
        {
            return BadRequest($"Username '{request.Username}' is already taken.");
        }

        User user = new User
        {
            Username = request.Username,
            Password = request.Password
        };

        User created = await userRepo.AddAsync(user);
        return Created($"/Users/{created.Id}", ToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        User existing;
        try
        {
            existing = await userRepo.GetSingleAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"User with id {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("Username cannot be empty.");
        }

        bool nameChanged = !request.Username.Equals(existing.Username, StringComparison.OrdinalIgnoreCase);
        if (nameChanged && await UsernameTakenAsync(request.Username))
        {
            return BadRequest($"Username '{request.Username}' is already taken.");
        }

        existing.Username = request.Username;
        existing.Password = request.Password;

        await userRepo.UpdateAsync(existing);
        return Ok(ToDto(existing));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetSingleUser([FromRoute] int id)
    {
        try
        {
            User user = await userRepo.GetSingleAsync(id);
            return Ok(ToDto(user));
        }
        catch (InvalidOperationException)
        {
            return NotFound($"User with id {id} not found.");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] string? usernameContains)
    {
        IQueryable<User> query = userRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(usernameContains))
        {
            string filter = usernameContains.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(filter));
        }

        List<User> users = await query.OrderBy(u => u.Id).ToListAsync();
        return Ok(users.Select(ToDto).ToList());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        try
        {
            await userRepo.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound($"User with id {id} not found.");
        }
    }

    private async Task<bool> UsernameTakenAsync(string username)
    {
        string lowered = username.ToLower();
        return await userRepo.GetManyAsync().AnyAsync(u => u.Username.ToLower() == lowered);
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username
        };
    }
}
