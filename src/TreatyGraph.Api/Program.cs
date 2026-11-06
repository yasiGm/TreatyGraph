using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using TreatyGraph.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Do not advertise the web server in a "Server" header.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Behind the Azure Container Apps ingress the real caller is in X-Forwarded-For. Without this every
// request would look like it came from the ingress and the rate limit below would hit everyone at once.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// At most 300 requests a minute per caller; more gets "429 Too Many Requests".
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string ConnectionStrings:Default");

builder.Services.AddSingleton(new Db(connectionString));
builder.Services.AddSingleton<Repository>();

var app = builder.Build();

app.UseForwardedHeaders();

// Never show stack traces or internal details to callers.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp => errorApp.Run(context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(new { error = "Something went wrong." });
    }));
}

app.UseRateLimiter();

// Security headers on every response.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; " +
        "connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
    if (context.Request.IsHttps)
    {
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }

    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/meta", async (Repository repo) => Results.Ok(await repo.GetMetaAsync()));

app.MapGet("/api/countries/{ccode:int}", async (int ccode, Repository repo) =>
{
    if (ccode < 1 || ccode > 999)
    {
        return Results.BadRequest("COW country codes are between 1 and 999.");
    }

    var profile = await repo.GetProfileAsync(ccode);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
});

app.MapGet("/api/pairs/{a:int}/{b:int}", async (int a, int b, Repository repo) =>
{
    if (a < 1 || a > 999 || b < 1 || b > 999)
    {
        return Results.BadRequest("COW country codes are between 1 and 999.");
    }

    return Results.Ok(await repo.GetPairAsync(a, b));
});

app.Run();
