using RepositoryContracts;

namespace ConsoleApp.UI;

public class CliApp
{
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public CliApp(IUserRepository userRepository, IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }

    public async Task StartAsync()
    {
        Console.WriteLine("Welcome to the Forum CLI");

        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("=== MAIN MENU ===");
            Console.WriteLine("1) Manage users");
            Console.WriteLine("2) Manage posts");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await new ManageUsersView(userRepository).ShowAsync();
                    break;
                case "2":
                    await new ManagePostsView(postRepository, userRepository, commentRepository).ShowAsync();
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Unknown choice, try again.");
                    break;
            }
        }

        Console.WriteLine("Goodbye!");
    }
}
