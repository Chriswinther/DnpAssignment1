using ApiContracts.Posts;

namespace BlazorApp.Services;

public interface IPostService
{
    Task<PostDto> AddPostAsync(CreatePostDto request);
    Task<List<PostDto>> GetPostsAsync(string? titleContains = null, int? userId = null);
    Task<PostWithCommentsDto> GetSinglePostAsync(int id);
    Task DeletePostAsync(int id);
}
