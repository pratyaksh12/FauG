using System;
using FauG.Gateway.Core.Data;
using FauG.Gateway.Core.Entities;
using FauG.Gateway.Core.Middleware;
using Microsoft.EntityFrameworkCore;

namespace FauG.Gateway.Core.Services;

public class RequestLogService(RedisService redis, IServiceScopeFactory scopeFactory)
{
    public async Task<ModelCost?> GetModelCostAsync(string modelName)
    {
        var cacheKey = $"Cost:{modelName}";
        var model = await redis.GetAsync<ModelCost>(cacheKey);
        if(model is not null) return model;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        model = await db.ModelCost.FirstOrDefaultAsync<ModelCost?>(model => model!.ModelName == modelName);

        if(model is null) return null;

        await redis.SetAsync(cacheKey, model, TimeSpan.FromHours(1));
        return model;

    }

    public async Task LogUsageAsync(AuthContext auth, string modelName, int totalTokens, int statusCode)
    {
        var modelData = await GetModelCostAsync(modelName);
        var costPerToken = modelData?.InputCostPer1k/1000m ?? 0m;
        var totalCost = totalTokens * costPerToken;

        await redis.DecrementAsync($"Budget:{auth.UserId}", (long)totalCost*10000);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var requestLog = new RequestLog
        {
            ModelName = modelName,
            TotalTokens = totalTokens,
            EstimatedCost = totalCost,
            StatusCode = statusCode,
            VirtualKeyId = auth.VirtualKeyId,
        };

        db.RequestLogs.Add(requestLog);
        var user = await db.Users.FindAsync(auth.UserId);
        if(user is not null) user.CurrentSpend += totalCost;

        await db.SaveChangesAsync();
    }
}
