using ApiContracts.Auth;
using ApiContracts.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public AuthController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized("Username and password are required.");
        }

        string lowered = request.Username.ToLower();
        User? user = await userRepo.GetManyAsync()
            .SingleOrDefaultAsync(u => u.Username.ToLower() == lowered);

        if (user is null || user.Password != request.Password)
        {
            return Unauthorized("Wrong username or password.");
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username
        });
    }
}
