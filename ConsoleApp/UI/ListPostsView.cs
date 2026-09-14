using Entities;
using RepositoryContracts;

namespace ConsoleApp.UI;

public class ListPostsView
{
    private readonly IPostRepository postRepository;

    public ListPostsView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public void Show()
    {
        Console.WriteLine();
        Console.WriteLine("--- POSTS OVERVIEW ---");

        List<Post> posts = postRepository.GetManyAsync().OrderBy(p => p.Id).ToList();
        if (posts.Count == 0)
        {
            Console.WriteLine("No posts yet.");
            return;
        }

        foreach (Post post in posts)
        {
            Console.WriteLine($"[{post.Id}] {post.Title}");
        }
    }
}
