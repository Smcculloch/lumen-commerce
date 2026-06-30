using Lumen.Application.Categories;
using Lumen.Application.Content;
using Lumen.Application.Customers;
using Lumen.Application.Orders;
using Lumen.Application.Products;
using Lumen.Application.Templates;
using Lumen.Application.Templates.Management;
using Lumen.Application.Templates.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Lumen.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLumenApplication(this IServiceCollection services)
    {
        services.AddSingleton<ICodeTemplateProvider, LumenCodeTemplateProvider>();
        services.AddScoped<TemplateValidator>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IContentItemService, ContentItemService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ITemplateManagementService, TemplateManagementService>();

        return services;
    }
}