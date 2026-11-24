using EfcRepositories;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

string connectionString = builder.Configuration.GetConnectionString("AppDb")
                          ?? "Data Source=app.db";

builder.Services.AddDbContext<EfcRepositories.AppContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUserRepository, EfcUserRepository>();
builder.Services.AddScoped<IPostRepository, EfcPostRepository>();
builder.Services.AddScoped<ICommentRepository, EfcCommentRepository>();

var app = builder.Build();

app.MapControllers();

app.Run();
