using Entities;
using RepositoryContracts;

namespace ConsoleApp.UI;

public class AddCommentView
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public AddCommentView(ICommentRepository commentRepository, IPostRepository postRepository,
        IUserRepository userRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- ADD COMMENT ---");

        Console.Write("Post id: ");
        if (!int.TryParse(Console.ReadLine(), out int postId))
        {
            Console.WriteLine("Post id must be a number.");
            return;
        }

        if (!postRepository.GetManyAsync().Any(p => p.Id == postId))
        {
            Console.WriteLine($"No post with id {postId}.");
            return;
        }

        Console.Write("Your user id: ");
        if (!int.TryParse(Console.ReadLine(), out int userId))
        {
            Console.WriteLine("User id must be a number.");
            return;
        }

        if (!userRepository.GetManyAsync().Any(u => u.Id == userId))
        {
            Console.WriteLine($"No user with id {userId}.");
            return;
        }

        Console.Write("Comment: ");
        string? body = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Comment cannot be empty.");
            return;
        }

        Comment created = await commentRepository.AddAsync(new Comment
        {
            Body = body,
            PostId = postId,
            UserId = userId
        });

        Console.WriteLine($"Comment created with id {created.Id}.");
    }
}
