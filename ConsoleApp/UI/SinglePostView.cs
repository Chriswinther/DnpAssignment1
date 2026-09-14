using Entities;
using RepositoryContracts;

namespace ConsoleApp.UI;

public class SinglePostView
{
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IUserRepository userRepository;

    public SinglePostView(IPostRepository postRepository, ICommentRepository commentRepository,
        IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine();
        Console.Write("Post id: ");
        if (!int.TryParse(Console.ReadLine(), out int postId))
        {
            Console.WriteLine("Post id must be a number.");
            return;
        }

        Post post;
        try
        {
            post = await postRepository.GetSingleAsync(postId);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"=== [{post.Id}] {post.Title} ===");
        Console.WriteLine($"By: {GetUsername(post.UserId)}");
        Console.WriteLine(post.Body);
        Console.WriteLine();
        Console.WriteLine("Comments:");

        List<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => c.PostId == post.Id)
            .OrderBy(c => c.Id)
            .ToList();

        if (comments.Count == 0)
        {
            Console.WriteLine("  (no comments yet)");
            return;
        }

        foreach (Comment comment in comments)
        {
            Console.WriteLine($"  [{comment.Id}] {GetUsername(comment.UserId)}: {comment.Body}");
        }
    }

    private string GetUsername(int userId)
    {
        User? user = userRepository.GetManyAsync().SingleOrDefault(u => u.Id == userId);
        return user is null ? $"unknown user ({userId})" : user.Username;
    }
}
