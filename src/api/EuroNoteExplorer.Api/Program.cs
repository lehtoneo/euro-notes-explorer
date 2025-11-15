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

builder.Services.AddInMemoryCache();
builder.Services.AddEuroNoteService(euroNoteServiceOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
