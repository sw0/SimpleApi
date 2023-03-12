using Microsoft.AspNetCore.Mvc;
using SimpleApi.Models;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;

namespace SimpleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IRedisDatabase redisDatabase;
        private readonly IRedisConnectionPoolManager pool;

        public PostController(IRedisDatabase redisDatabase, IRedisConnectionPoolManager pool)
        {
            this.redisDatabase = redisDatabase;
            this.pool = pool;
        }

        [HttpGet]
        public async Task<string> IndexAsync()
        {
            var before = pool.GetConnectionInformation();
            var rng = new Random();
            await redisDatabase.AddAsync($"key-{rng}", new User { Id = rng.Next(), Name = $"User.{rng.Next()}" }).ConfigureAwait(false);

            var after = pool.GetConnectionInformation();

            return BuildInfo(before) + "\t" + BuildInfo(after);

            static string BuildInfo(ConnectionPoolInformation info)
            {
                return $"alive: {info.ActiveConnections.ToString()}, required: {info.RequiredPoolSize.ToString()}";
            }
        }

        [HttpGet]
        [Route("get")]
        public async Task<User> GetAsync(string key)
        {
            var result = await redisDatabase.GetAsync<User>(key).ConfigureAwait(false);

            return result!;
        }

        [HttpPost]
        [Route("set")]
        public async Task<bool> SetAsync(string key, int id, string name)
        {
            var result = await redisDatabase.AddAsync<User>(key, new User { Id = id, Name = name }).ConfigureAwait(false);

            return result;
        }
    }
}