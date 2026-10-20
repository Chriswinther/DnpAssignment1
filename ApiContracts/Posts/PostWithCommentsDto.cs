using ApiContracts.Comments;

namespace ApiContracts.Posts;

public class PostWithCommentsDto : PostDto
{
    public List<CommentDto> Comments { get; set; } = new();
}
