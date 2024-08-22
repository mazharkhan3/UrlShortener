using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using UrlShortener;
using UrlShortener.Entities;
using UrlShortener.Extensions;
using UrlShortener.Models;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddScoped<UrlShorteningService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.MapPost("api/shorten", (
        ShortenUrlRequest request,
        AppDbContext dbContext,
        HttpContext httpContext
    ) =>
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            return Results.BadRequest("Invalid Url");
        }

        var code = Guid.NewGuid().ToString().Substring(0, 6);
        
        var shortenedUrl = new ShortenedUrl
        {
            LongUrl = request.Url,
            Code = code,
            ShortUrl =
                $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}"
        };
        
        dbContext.ShortenedUrls.Add(shortenedUrl);
        
        dbContext.SaveChanges();

        return Results.Ok(shortenedUrl.ShortUrl);
    });

app.MapGet("api/{code}", (
        string code,
        AppDbContext dbContext
    ) =>
{
    var shortenedUrl = dbContext.ShortenedUrls.FirstOrDefault(x => x.Code == code);

    if (shortenedUrl == null)
    {
        return Results.NotFound();
    }

    return Results.Redirect(shortenedUrl.LongUrl);
});

app.Run();
