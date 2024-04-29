using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Domain.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace BasketService.Api.Infrastructure.Repository;

public class RedisBasketRepository : IBasketRepository
{
    private readonly ILogger<RedisBasketRepository> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisBasketRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
    {
        _logger = loggerFactory.CreateLogger<RedisBasketRepository>();
        _redis = redis;
        _database = _redis.GetDatabase();
    }

    public async Task<bool> DeleteBasketAsync(string userName)
    {
        return await _database.KeyDeleteAsync(userName);
    }

    public async Task<CustomerBasket> GetBasketAsync(string userName)
    {
        var basket = await _database.StringGetAsync(userName);

        if (basket.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<CustomerBasket>(basket)!;
    }

    public IEnumerable<string> GetUsers()
    {
        var data = GetServer().Keys();
        return data.Select(x => x.ToString());
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        var created = await _database.StringSetAsync(basket.BuyerId, JsonSerializer.Serialize(basket));

        if (!created)
        {
            _logger.LogInformation("Problem occur persisting the item.");
            return null;
        }

        _logger.LogInformation("Basket item persisted successfully.");

        return await GetBasketAsync(basket.BuyerId);
    }

    private IServer GetServer()
    {
        var endpoints = _redis.GetEndPoints();
        return _redis.GetServer(endpoints.First());
    }
}
