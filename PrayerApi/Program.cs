using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClientFactory
builder.Services.AddHttpClient("aladhan")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

// Memory cache (simple caching)
builder.Services.AddMemoryCache();

// CORS - allow Angular dev server
var allowedOrigins = new string[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("DevCors");

app.MapControllers();

app.Run();
