using FauG.Gateway.Core.Data;
using FauG.Gateway.Core.Middleware;
using FauG.Gateway.Core.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using DotNetEnv;
Env.TraversePath().Load();

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
// Add redis connection for atomic operations
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")!)
);
// Registering routing logic
builder.Services.AddSingleton<RoutingTransformProvider>();
// register wrapper service
builder.Services.AddSingleton<RedisService>();
// register async request logger
builder.Services.AddSingleton<RequestLogService>();
// register jailbreakscanner - Qualfire's Sentinel-v2
builder.Services.AddSingleton<JailbreakService>();
// YARP(Reverse Proxy)
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy")).AddTransforms<RoutingTransformProvider>();
var app = builder.Build();

// reegister middleware
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<BudgetMiddleware>();
app.UseMiddleware<SecurityMiddleware>();

// Configure the HTTP request pipeline.
app.MapReverseProxy();

app.MapGet("/", () =>{
   return "FauG Active Hiii!!";
});

app.UseHttpsRedirection();
app.Run();

