using Entities;
using RepositoryContracts;

namespace ConsoleApp.UI;

public class ListUsersView
{
    private readonly IUserRepository userRepository;

    public ListUsersView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public void Show()
    {
        Console.WriteLine();
        Console.WriteLine("--- ALL USERS ---");

        List<User> users = userRepository.GetManyAsync().OrderBy(u => u.Id).ToList();
        if (users.Count == 0)
        {
            Console.WriteLine("No users yet.");
            return;
        }

        foreach (User user in users)
        {
            Console.WriteLine($"[{user.Id}] {user.Username}");
        }
    }
}
