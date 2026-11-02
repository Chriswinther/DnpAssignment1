using ApiContracts.Auth;
using ApiContracts.Users;
using Entities;
using Microsoft.AspNetCore.Mvc;
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
    public ActionResult<UserDto> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized("Username and password are required.");
        }

        User? user = userRepo.GetManyAsync()
            .SingleOrDefault(u => u.Username.ToLower() == request.Username.ToLower());

        if (user is null || user.Password != request.Password)
        {
            return Unauthorized("Wrong username or password.");
        }

        UserDto dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username
        };

        return Ok(dto);
    }
}
