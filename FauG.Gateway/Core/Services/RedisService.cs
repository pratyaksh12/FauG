using System;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace FauG.Gateway.Core.Services;

public class RedisService(IDistributedCache cache, IConnectionMultiplexer redisMux)
{
    private readonly IDatabase _db = redisMux.GetDatabase();

    // Caching Objects with Auth Policies
    public async Task<T?> GetTAsync<T>(string key)
    {
        var data = await cache.GetStringAsync(key);
        return data == null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan tte)
    {
        var data = System.Text.Json.JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, data, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = tte
        });
    }

    // Atomic Counter for Budget and Rate Limiting
    public async Task<long> DecrementAsync(string key, long amount = 1)
    {
        return await _db.StringDecrementAsync(key,amount);
    }
    public async Task<long> GetCounterAsync(string key)
    {
        var val = await _db.StringGetAsync(key);
        return val.HasValue ? (long)val : 0;
    }
}
