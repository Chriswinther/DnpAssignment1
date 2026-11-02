using ApiContracts.Users;

namespace BlazorApp.Services;

public interface IUserService
{
    Task<UserDto> AddUserAsync(CreateUserDto request);
    Task<List<UserDto>> GetUsersAsync(string? usernameContains = null);
    Task<UserDto> GetSingleUserAsync(int id);
}
