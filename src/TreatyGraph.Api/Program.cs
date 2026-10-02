using TreatyGraph.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string ConnectionStrings:Default");

builder.Services.AddSingleton(new Db(connectionString));
builder.Services.AddSingleton<Repository>();

var app = builder.Build();

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
