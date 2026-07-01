using Lumen.Backoffice.Data;
using Lumen.Infrastructure;
using Lumen.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var databasePath = DatabasePath.Resolve(builder.Environment.ContentRootPath);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? DatabasePath.ToConnectionString(databasePath);
builder.Services.AddLumenInfrastructure(connectionString, builder.Configuration);
builder.Services.AddLumenScheduledJobs(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Lumen", LogLevel.Information);
    Console.WriteLine($"[Lumen] SQLite database: {databasePath}");
}

var app = builder.Build();

await app.Services.InitializeLumenDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
