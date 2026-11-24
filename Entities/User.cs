using System.Text.Json.Serialization;

namespace Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    [JsonIgnore]
    public List<Post> Posts { get; set; } = new();

    [JsonIgnore]
    public List<Comment> Comments { get; set; } = new();
}
