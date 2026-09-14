namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; }
    public int PostId { get; set; }   // fremmednøgle til Post
    public int UserId { get; set; }   // fremmednøgle til User
}  
