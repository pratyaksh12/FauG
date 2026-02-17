using FauG.Gateway.Core.Data;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Add application persistance for default database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Add redis caching to project
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// YARP(Reverse Proxy)
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapReverseProxy();

app.MapGet("/", () =>{
   return "FauG Active Hiii!!";
});

app.UseHttpsRedirection();
app.Run();

