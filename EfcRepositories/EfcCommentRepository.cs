using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcCommentRepository : ICommentRepository
{
    private readonly AppContext ctx;

    public EfcCommentRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        await ctx.Comments.AddAsync(comment);
        await ctx.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        bool exists = await ctx.Comments.AnyAsync(c => c.Id == comment.Id);
        if (!exists)
        {
            throw new InvalidOperationException($"Comment with ID '{comment.Id}' not found");
        }

        ctx.Comments.Update(comment);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Comment? existing = await ctx.Comments.SingleOrDefaultAsync(c => c.Id == id);
        if (existing is null)
        {
            throw new InvalidOperationException($"Comment with ID '{id}' not found");
        }

        ctx.Comments.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        Comment? comment = await ctx.Comments.SingleOrDefaultAsync(c => c.Id == id);
        if (comment is null)
        {
            throw new InvalidOperationException($"Comment with ID '{id}' not found");
        }

        return comment;
    }

    public IQueryable<Comment> GetManyAsync()
    {
        return ctx.Comments.AsQueryable();
    }
}
