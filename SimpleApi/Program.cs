using SimpleApi.Extensions;
using StackExchange.Redis.Extensions.Core.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRedis(builder.Configuration);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRedisInformation();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


//var redisDatabase = app.Services.GetRequiredService<IRedisDatabase>();

//var obj = new ValueTypeRedisItem<bool>(true);

//redisDatabase.AddAsync("myCacheKey", obj)
//    .GetAwaiter()
//    .GetResult();

//var result = redisDatabase.GetAsync<ValueTypeRedisItem<bool>>("myCacheKey")
//    .GetAwaiter()
//    .GetResult();
app.MapPing();
app.MapApiPing();
app.Run();

/// <summary>
/// The container class for value types
/// </summary>
/// <typeparam name="T"></typeparam>
public class ValueTypeRedisItem<T> where T : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueTypeRedisItem{T}"/> class.
    /// </summary>
    /// <param name="value">The Value</param>
    public ValueTypeRedisItem(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Return the specified value
    /// </summary>
    public T Value { get; }
}
