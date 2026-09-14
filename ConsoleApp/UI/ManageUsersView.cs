using RepositoryContracts;

namespace ConsoleApp.UI;

public class ManageUsersView
{
    private readonly IUserRepository userRepository;

    public ManageUsersView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--- MANAGE USERS ---");
            Console.WriteLine("1) Create new user");
            Console.WriteLine("2) See all users");
            Console.WriteLine("0) Back");
            Console.Write("Choose: ");

            switch (Console.ReadLine())
            {
                case "1":
                    await new CreateUserView(userRepository).ShowAsync();
                    break;
                case "2":
                    new ListUsersView(userRepository).Show();
                    break;
                case "0":
                    back = true;
                    break;
                default:
                    Console.WriteLine("Unknown choice, try again.");
                    break;
            }
        }
    }
}
