using Lumen.Infrastructure;
using Lumen.Infrastructure.Identity;
using Lumen.Infrastructure.Persistence;
using Lumen.Storefront;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "Lumen.Session";
});

var databasePath = DatabasePath.Resolve(builder.Environment.ContentRootPath);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? DatabasePath.ToConnectionString(databasePath);
builder.Services.AddLumenInfrastructure(connectionString);
builder.Services.AddLumenIdentity();
builder.Services.AddLumenStorefrontCart();

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine($"[Lumen] SQLite database: {databasePath}");
}

var app = builder.Build();

await app.Services.InitializeLumenDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();