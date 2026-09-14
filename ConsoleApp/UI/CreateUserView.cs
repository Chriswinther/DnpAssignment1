using Entities;
using RepositoryContracts;

namespace ConsoleApp.UI;

public class CreateUserView
{
    private readonly IUserRepository userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- CREATE USER ---");

        Console.Write("Username: ");
        string? username = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("Username cannot be empty.");
            return;
        }

        bool taken = userRepository.GetManyAsync()
            .Any(u => u.Username.ToLower() == username.ToLower());
        if (taken)
        {
            Console.WriteLine($"Username '{username}' is already taken.");
            return;
        }

        Console.Write("Password: ");
        string? password = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Password cannot be empty.");
            return;
        }

        User created = await userRepository.AddAsync(new User
        {
            Username = username,
            Password = password
        });

        Console.WriteLine($"User created with id {created.Id}.");
    }
}
