using System;
using FauG.Gateway.Core.Data;
using FauG.Gateway.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FauG.Gateway.Core.Middleware;

public class AuthMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, RedisService redis)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if(!context.Request.Headers.TryGetValue("Authorization", out var tokenValue))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing API key");
            return;
        }

        var keyHash = tokenValue.ToString().Replace("Bearer", "");

        // hot path check
        var cacheAuth = await redis.GetAsync<AuthContext>($"Auth:{keyHash}");

        if(cacheAuth is null)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var key = await db.VirtualKeys.Include(k => k.User).ThenInclude(u => u.Organisation).FirstOrDefaultAsync(k => k.KeyHash == keyHash && !k.IsRevoked);

            if(key is null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid key.");
                return;
            }
            var cachedAuth = new AuthContext(key.UserId, key.User.OrganisationId, key.Id, key.User.AllocatedBudget);
            await redis.SetAsync($"Auth{keyHash}", cachedAuth, TimeSpan.FromMinutes(5));
        }

        context.Items["Auth"] = cacheAuth;
        await next(context);
    }
}

public record AuthContext(Guid UserId, Guid OrgId, Guid VirtualKeyId, decimal AllocatedBudget);
