using Lumen.Application;
using Lumen.Application.Cart;
using Lumen.Application.Categories;
using Lumen.Application.Content;
using Lumen.Application.Customers;
using Lumen.Application.Orders;
using Lumen.Application.Media;
using Lumen.Application.Products;
using Lumen.Application.Templates;
using Lumen.Application.Templates.Management;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Seed;
using Lumen.Infrastructure.Repositories;
using Lumen.Infrastructure.Payments;
using Lumen.Infrastructure.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lumen.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLumenInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddLumenApplication();
        services.AddLumenPayments(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ITemplateRegistry, TemplateRegistry>();
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }

    public static async Task InitializeLumenDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var environment = scope.ServiceProvider.GetService<IHostEnvironment>();
        await DatabaseInitializer.MigrateAsync(dbContext, environment);
        await TemplateSeedData.SeedAsync(dbContext);
        await ContentSeedData.SeedAsync(dbContext);
        await PimSeedData.SeedAsync(dbContext);
    }
}