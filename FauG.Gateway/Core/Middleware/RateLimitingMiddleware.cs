using System;
using FauG.Gateway.Core.Services;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace FauG.Gateway.Core.Middleware;

public class RateLimitingMiddleware(RequestDelegate next)
{
    private readonly int maxRateLimit = 10;

    public async Task InvokeAsync(HttpContext context, RedisService redis)
    {
        if (context.Request.Path.StartsWithSegments("/v1/chat/completions"))
        {
            if(context.Items.TryGetValue("Auth", out var authData) && authData is AuthContext authContext)
            {
                var currentMinute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                var rateLimitKey = $"RateLimit:{authContext.UserId}:{currentMinute}";

                var currentCount = await redis.IncrementAsync(rateLimitKey);

                if(currentCount == 1)
                {
                    await redis.SetExpiryAsync(rateLimitKey, TimeSpan.FromMinutes(2));
                }

                int remaining = Math.Max(0, maxRateLimit - currentCount);
                context.Response.Headers.Append("X-Rate-Limit", maxRateLimit.ToString());
                context.Response.Headers.Append("X-RateLimit-Remaining", remaining.ToString());

                if(currentCount > maxRateLimit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\" :\"Rate Limit Exceeded\"}");
                    return;
                }
            }
        }

        await next(context);
    }
}
