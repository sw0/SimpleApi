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
        private readonly IRedisDatabase _redisDatabase;
        private readonly IRedisConnectionPoolManager _pool;
        private readonly ILogger<PostController> _logger;

        public PostController(IRedisDatabase redisDatabase, IRedisConnectionPoolManager pool, ILogger<PostController> logger)
        {
            _redisDatabase = redisDatabase;
            _pool = pool;
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> IndexAsync()
        {
            _logger.LogInformation("Api PostController.Index got called."); //to see the log in container insights

            var before = _pool.GetConnectionInformation();
            var rng = new Random();
            await _redisDatabase.AddAsync($"key-{rng}", new User { Id = rng.Next(), Name = $"User.{rng.Next()}" }).ConfigureAwait(false);

            var after = _pool.GetConnectionInformation();

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
            _logger.LogInformation("Api PostController.GetAsync got called."); //to see the log in container insights

            var result = await _redisDatabase.GetAsync<User>(key).ConfigureAwait(false);

            return result!;
        }

        [HttpPost]
        [Route("set")]
        public async Task<bool> SetAsync(string key, int id, string name)
        {
            _logger.LogInformation("Api PostController.SetAsync got called. key: {0}, id: {1} and name: {2}", key, id, name); //to see the log in container insights
            var result = await _redisDatabase.AddAsync<User>(key, new User { Id = id, Name = name }).ConfigureAwait(false);

            return result;
        }
    }
}