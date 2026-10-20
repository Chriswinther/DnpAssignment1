namespace ApiContracts.Users;

public class UpdateUserDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
