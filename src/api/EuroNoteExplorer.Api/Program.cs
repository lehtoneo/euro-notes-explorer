using EuroNoteExplorer.Api.Options;
using EuroNoteExplorer.Api.Services;
using EuroNoteExplorer.Shared.Caching;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? bofOpenAPIBaseUrl = builder.Configuration["BoFOpenAPIBaseUrl"] ?? throw new ArgumentNullException("BoFOpenAPIBaseUrl configuration is null or empty");
EuroNoteServiceOptions euroNoteServiceOptions = new EuroNoteServiceOptions { 
    BoFOpenAPIBaseUrl = bofOpenAPIBaseUrl
};

string? redisConnectionString = builder.Configuration["RedisConnectionString"] ?? null;


if (redisConnectionString == null)
{
    Console.WriteLine("No RedisConnectionString configured. Using InMemory cache");
	builder.Services.AddInMemoryCache();
} else
{
	Console.WriteLine("RedisConnectionString configured. Using Redis as cache");
	builder.Services.AddRedisCache(new RedisOptions { ConnectionString = redisConnectionString });
}

builder.Services.AddEuroNoteService(euroNoteServiceOptions);

var app = builder.Build();

app.Logger.LogInformation("App starting...");
app.Logger.LogInformation($"Using Redis as cache: {redisConnectionString != null}");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class accessible to tests
public partial class Program { }
