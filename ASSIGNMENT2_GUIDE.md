# Assignment 2: Command Line Interface, step by step

Solution: `C:\Users\chris\Downloads\DnpAssignment1-master\DnpAssignment1-master\DnpAssignment1.sln`
IDE: JetBrains Rider (your project already has a `.idea` folder)

What you have now: `Entities`, `RepositoryContracts`, `InMemoryRepositories`, all targeting **net10.0**.
What you need to add: a `ConsoleApp` console project (done), dummy data in the three in-memory repositories, and the UI classes.

---

## Step 0: Open the solution

1. Open Rider.
2. File > Open, pick `DnpAssignment1.sln`.
3. Wait for it to finish loading all three projects in the Solution view on the left.

---

## Step 1: Create the console project (DONE)

You already did this: the project is called **`ConsoleApp`**, it sits at
`...\DnpAssignment1-master\ConsoleApp`, it targets **net10.0**, it is nested under the `Server`
solution folder, and it prints `Hello, World!` when you run it. That is exactly right.

Because you named it `ConsoleApp` and not `CLI`, the root namespace is `ConsoleApp`. Every code
block below already uses that name, so just copy-paste as is.

If you would rather have it called `CLI`: right-click the project > Refactor > Rename, tick
"rename project folder" and "rename namespace". Not required, the name is free choice.

---

## Step 2: Add the project references

1. Right-click the **`ConsoleApp`** project > **Add > Reference...**
2. Tick all three:
   - `Entities`
   - `RepositoryContracts`
   - `InMemoryRepositories`
3. Click **Add**.

**File to check afterwards:** `ConsoleApp/ConsoleApp.csproj`. It should now look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\Entities\Entities.csproj" />
        <ProjectReference Include="..\RepositoryContracts\RepositoryContracts.csproj" />
        <ProjectReference Include="..\InMemoryRepositories\InMemoryRepositories.csproj" />
    </ItemGroup>

</Project>
```

Your project sits next to `Entities`, so those `..\` paths are correct. That is fine as long as they point at the right `.csproj` files.

---

## Step 3: Add dummy data to the repositories

The assignment requires 3 to 5 entities per repository at startup. You add a constructor to each existing repository class. The ids must line up: user 1 to 4 exist, so posts and comments can refer to them.

### File to open: `InMemoryRepositories/UserInMemoryRepository.cs`

Add this constructor right under `private readonly List<User> users = new();`

```csharp
    public UserInMemoryRepository()
    {
        _ = AddAsync(new User { Username = "christoffer", Password = "pass123" });
        _ = AddAsync(new User { Username = "emma", Password = "hunter2" });
        _ = AddAsync(new User { Username = "axel", Password = "qwerty" });
        _ = AddAsync(new User { Username = "samuel", Password = "dnp2025" });
    }
```

`_ =` discards the returned `Task`. That is safe here only because `AddAsync` finishes synchronously (it just returns `Task.FromResult`). You cannot `await` inside a constructor.

### File to open: `InMemoryRepositories/PostInMemoryRepository.cs`

Under `private readonly List<Post> posts = new();`

```csharp
    public PostInMemoryRepository()
    {
        _ = AddAsync(new Post { Title = "Welcome to the forum", Body = "This is the very first post.", UserId = 1 });
        _ = AddAsync(new Post { Title = "C# tips", Body = "Remember to await your async calls.", UserId = 2 });
        _ = AddAsync(new Post { Title = "Anyone going skiing?", Body = "Thinking about Chamonix in February.", UserId = 3 });
        _ = AddAsync(new Post { Title = "Exam questions", Body = "Where do we hand in the link?", UserId = 4 });
    }
```

### File to open: `InMemoryRepositories/CommentInMemoryRepository.cs`

Under `private readonly List<Comment> comments = new();`

```csharp
    public CommentInMemoryRepository()
    {
        _ = AddAsync(new Comment { Body = "Nice, welcome!", PostId = 1, UserId = 2 });
        _ = AddAsync(new Comment { Body = "Great first post.", PostId = 1, UserId = 3 });
        _ = AddAsync(new Comment { Body = "Turtles all the way down.", PostId = 2, UserId = 1 });
        _ = AddAsync(new Comment { Body = "I am in!", PostId = 3, UserId = 4 });
        _ = AddAsync(new Comment { Body = "On itslearning.", PostId = 4, UserId = 1 });
    }
```

---

## Step 4: Create the UI folder

Right-click the **`ConsoleApp`** project > **Add > New Directory**, name it **`UI`**.

Every class below goes in `ConsoleApp/UI/`. Create each one with right-click on the `UI` folder > **Add > Class/Interface > Class**, and name it exactly as shown. Rider will generate `namespace ConsoleApp.UI;` for you.

---

## Step 5: `ConsoleApp/UI/CliApp.cs`

This is the main menu loop. It receives all three repositories and passes them on.

```csharp
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
```

---

## Step 6: `ConsoleApp/UI/ManageUsersView.cs`

```csharp
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
```

---

## Step 7: `ConsoleApp/UI/CreateUserView.cs`

Covers the must-have "Create new user", plus a bit of the optional business logic (empty input, username already taken).

```csharp
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
```

---

## Step 8: `ConsoleApp/UI/ListUsersView.cs`

`GetManyAsync()` returns `IQueryable<User>`, not a `Task`, so there is nothing to await here. The method is plain `void Show()`.

```csharp
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
```

---

## Step 9: `ConsoleApp/UI/ManagePostsView.cs`

```csharp
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
```

---

## Step 10: `ConsoleApp/UI/CreatePostView.cs`

```csharp
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
```

---

## Step 11: `ConsoleApp/UI/ListPostsView.cs`

The must-have "View posts overview", so just title and id.

```csharp
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
```

---

## Step 12: `ConsoleApp/UI/SinglePostView.cs`

The must-have "View specific post": title, body, and the comments on it.

```csharp
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
```

---

## Step 13: `ConsoleApp/UI/AddCommentView.cs`

The must-have "Add comment to existing post".

```csharp
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
```

---

## Step 14: `ConsoleApp/Program.cs`

Open the file Rider generated and replace everything in it. This is the **only** place where you write `new ...InMemoryRepository()`. That is the point the assignment is strict about.

```csharp
using ConsoleApp.UI;
using InMemoryRepositories;
using RepositoryContracts;

Console.WriteLine("Starting CLI app...");

IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();

CliApp cliApp = new CliApp(userRepository, postRepository, commentRepository);
await cliApp.StartAsync();
```

Note the variable types are the **interfaces** (`IUserRepository`), not the concrete classes. That is the Dependency Inversion Principle, and next assignment you only swap these three lines.

The `await` at top level works because Program.cs uses top level statements, so the compiler turns it into `static async Task Main()` for you.

---

## Step 15: Build and run

1. Build > Build Solution (Ctrl+Shift+F9 in Rider, or the hammer icon).
2. Fix any red squiggles. The usual suspects:
   - Wrong framework on the ConsoleApp project (`net9.0` instead of `net10.0`)
   - A missing `using Entities;` or `using RepositoryContracts;` at the top of a view class
   - A class in `ConsoleApp/UI/` with namespace `ConsoleApp` instead of `ConsoleApp.UI`
3. Make sure the run configuration dropdown at the top right says **ConsoleApp**, then press the green Run arrow.

**Test script to verify every must-have requirement:**

| Do this | Expect |
|---|---|
| `2` then `2` | Overview lists the 4 dummy posts as `[id] title` |
| `3`, then post id `1` | Title, body, author, and 2 comments |
| `0`, then `1`, then `2` | The 4 dummy users |
| `1` (create user), username `testuser`, password `abc` | "User created with id 5." |
| `1` again, username `testuser` | "Username 'testuser' is already taken." |
| `0`, `2`, `1` (create post), title/body, user id `5` | "Post created with id 5." |
| `4` (add comment), post id `5`, user id `1`, some text | "Comment created with id 6." |
| `3`, post id `5` | Your new post with your new comment underneath |
| `3`, post id `99` | "Post with ID '99' not found" |

---

## Step 16: Push to GitHub and hand in

Your folder is an unzipped copy (`DnpAssignment1-master`) and has no `.git` folder, so it is not connected to GitHub yet. Two options:

**A. You already have the repo on GitHub:** clone it fresh somewhere else, copy your `ConsoleApp` folder and the three edited repository files into the clone, then commit and push from there.

**B. Start from this folder:** in Rider, VCS > Enable Version Control Integration > Git, then VCS > Share Project on GitHub. The existing `.gitignore` already excludes `bin/` and `obj/`, which is what you want.

Then:

1. Commit everything, push.
2. On github.com, navigate **into the `ConsoleApp` folder** of your repo.
3. Copy the URL from the browser address bar (it looks like `https://github.com/<user>/<repo>/tree/main/ConsoleApp`).
4. Hand that link in on itslearning.

---

## Checklist against the assignment

- [ ] Console Application project created under the Server solution folder
- [ ] ConsoleApp references Entities, RepositoryContracts, InMemoryRepositories
- [ ] Create new user (username, password)
- [ ] Create new post (title, body, user id)
- [ ] Add comment to existing post (body, user id, post id)
- [ ] View posts overview (title and id only)
- [ ] View specific post (title, body, comments)
- [ ] Repositories instantiated **only** in Program.cs, passed through constructors
- [ ] Fields typed as the interface, not the implementation
- [ ] 3 to 5 dummy entities in each repository
- [ ] Async used all the way up to the implicit main method
- [ ] Pushed to GitHub, link to the ConsoleApp folder handed in

## Optional extras if you want to go further

- Update and delete for users and posts (the repository methods already exist)
- Filters: all posts by a user id, all comments by a user id, users whose username contains a word
- A `ManageCommentsView` for full CRUD on comments
- Give `User`, `Post` and `Comment` constructors, or `= string.Empty;` on the string properties, to silence the nullable warnings
