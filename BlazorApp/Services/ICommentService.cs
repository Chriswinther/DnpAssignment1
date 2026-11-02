using ApiContracts.Comments;

namespace BlazorApp.Services;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(CreateCommentDto request);
    Task<List<CommentDto>> GetCommentsAsync(int? postId = null, int? userId = null);
}
