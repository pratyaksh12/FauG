using System;
using System.CodeDom;
using FauG.Gateway.Core.Services;

namespace FauG.Gateway.Core.Middleware;

public class BudgetMiddleware(RequestDelegate next, RedisService redis)
{

    public async Task InvokeAsync(HttpContext context)
    {
        if(context.Items["Auth"] is not AuthContext auth)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid session");
            return;
        }

        long currentSpendMicroCents = await redis.GetCounterAsync($"Spend:{auth.UserId}");
        decimal currentSpend = currentSpendMicroCents / 1_000_000m;

        // hard check
        if(currentSpend >= auth.AllocatedBudget)
        {
            context.Response.StatusCode = 402;
            await context.Response.WriteAsync($"Payment required, budget exceeded -> Allocated Budget: {auth.AllocatedBudget:F2} - Current Spend:{currentSpend:F2}");
            return;
        }

        await next(context);
    }

}
