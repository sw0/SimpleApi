using Microsoft.Extensions.Configuration;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace SimpleApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConfiguration = configuration.GetSection("Redis").Get<RedisConfiguration>();
            services.AddSingleton(redisConfiguration!);
            _ = services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(redisConfiguration!);
            services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();
            //services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            //services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            //services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
            //services.AddSingleton<StackExchange.Redis.Extensions.Core.ISerializer, StackExchange.Redis.Extensions.MsgPack.MsgPackObjectSerializer>();
        }

    }
}
