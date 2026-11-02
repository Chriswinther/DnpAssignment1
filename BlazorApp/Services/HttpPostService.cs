using System.Net.Http.Json;
using System.Text.Json;
using ApiContracts.Posts;

namespace BlazorApp.Services;

public class HttpPostService : IPostService
{
    private readonly HttpClient client;

    private static readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpPostService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<PostDto> AddPostAsync(CreatePostDto request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync("posts", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<PostDto>(response, options)!;
    }

    public async Task<List<PostDto>> GetPostsAsync(string? titleContains = null, int? userId = null)
    {
        List<string> query = new List<string>();
        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            query.Add($"titleContains={Uri.EscapeDataString(titleContains)}");
        }

        if (userId.HasValue)
        {
            query.Add($"userId={userId.Value}");
        }

        string uri = "posts";
        if (query.Count > 0)
        {
            uri += "?" + string.Join("&", query);
        }

        HttpResponseMessage httpResponse = await client.GetAsync(uri);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<List<PostDto>>(response, options)!;
    }

    public async Task<PostWithCommentsDto> GetSinglePostAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.GetAsync($"posts/{id}?includeComments=true");
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<PostWithCommentsDto>(response, options)!;
    }

    public async Task DeletePostAsync(int id)
    {
        HttpResponseMessage httpResponse = await client.DeleteAsync($"posts/{id}");
        if (!httpResponse.IsSuccessStatusCode)
        {
            string response = await httpResponse.Content.ReadAsStringAsync();
            throw new Exception(response);
        }
    }
}
