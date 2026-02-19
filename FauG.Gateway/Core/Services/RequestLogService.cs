using System;
using FauG.Gateway.Core.Data;
using FauG.Gateway.Core.Entities;
using FauG.Gateway.Core.Middleware;
using Microsoft.EntityFrameworkCore;

namespace FauG.Gateway.Core.Services;

public class RequestLogService(RedisService redis, IServiceScopeFactory scopeFactory)
{
    private async Task<ModelCost?> GetModelCostAsync(string model)
    {
        var cachedKey = $"Cost:{model}";
        var cached = await redis.GetAsync<ModelCost>(cachedKey);

        if(cached is null) return cached;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var modelData = await db.ModelCost.FirstOrDefaultAsync(mc => mc.ModelName == model);
        
        if(modelData is null) return null;

        await redis.SetAsync(cachedKey, modelData, TimeSpan.FromHours(1));

        return modelData;
    }
}
