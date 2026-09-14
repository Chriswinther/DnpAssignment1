using RepositoryContracts;

namespace ConsoleApp.UI;

public class ManagePostsView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public ManagePostsView(IPostRepository postRepository, IUserRepository userRepository,
        ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--- MANAGE POSTS ---");
            Console.WriteLine("1) Create new post");
            Console.WriteLine("2) See overview of posts");
            Console.WriteLine("3) View a single post");
            Console.WriteLine("4) Add comment to a post");
            Console.WriteLine("0) Back");
            Console.Write("Choose: ");

            switch (Console.ReadLine())
            {
                case "1":
                    await new CreatePostView(postRepository, userRepository).ShowAsync();
                    break;
                case "2":
                    new ListPostsView(postRepository).Show();
                    break;
                case "3":
                    await new SinglePostView(postRepository, commentRepository, userRepository).ShowAsync();
                    break;
                case "4":
                    await new AddCommentView(commentRepository, postRepository, userRepository).ShowAsync();
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
