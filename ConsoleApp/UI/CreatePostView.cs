using Entities;
using RepositoryContracts;

namespace ConsoleApp.UI;

public class CreatePostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CreatePostView(IPostRepository postRepository, IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- CREATE POST ---");

        Console.Write("Title: ");
        string? title = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Title cannot be empty.");
            return;
        }

        Console.Write("Body: ");
        string? body = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Body cannot be empty.");
            return;
        }

        Console.Write("Your user id: ");
        if (!int.TryParse(Console.ReadLine(), out int userId))
        {
            Console.WriteLine("User id must be a number.");
            return;
        }

        bool userExists = userRepository.GetManyAsync().Any(u => u.Id == userId);
        if (!userExists)
        {
            Console.WriteLine($"No user with id {userId}.");
            return;
        }

        Post created = await postRepository.AddAsync(new Post
        {
            Title = title,
            Body = body,
            UserId = userId
        });

        Console.WriteLine($"Post created with id {created.Id}.");
    }
}
