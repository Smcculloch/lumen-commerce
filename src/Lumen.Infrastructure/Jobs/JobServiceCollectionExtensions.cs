using Lumen.Application.Jobs;
using Lumen.Application.Jobs.Dtos;
using Lumen.Infrastructure.Jobs.Handlers;
using Lumen.Infrastructure.Jobs.Quartz;
using Lumen.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Lumen.Infrastructure.Jobs;

public static class JobServiceCollectionExtensions
{
    public static IServiceCollection AddLumenJobServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jobOptions = configuration.GetSection(JobOptions.SectionName).Get<JobOptions>() ?? new JobOptions();
        services.Configure<JobOptions>(configuration.GetSection(JobOptions.SectionName));

        JobRegistry.Initialize(
        [
            new JobDefinitionDto(
                LumenJobKeys.AbandonedCart,
                "Abandoned cart emails",
                "Queues reminder notifications for carts inactive with items.",
                jobOptions.AbandonedCart.CronExpression),
            new JobDefinitionDto(
                LumenJobKeys.CartCleanup,
                "Cart cleanup",
                "Deletes stale empty carts past the configured age.",
                jobOptions.CartCleanup.CronExpression),
            new JobDefinitionDto(
                LumenJobKeys.OrderStatus,
                "Order status reminders",
                "Queues status notifications for long-pending orders.",
                jobOptions.OrderStatus.CronExpression),
            new JobDefinitionDto(
                LumenJobKeys.InventorySync,
                "Inventory sync (stub)",
                "Placeholder for future inventory synchronization.",
                jobOptions.InventorySync.CronExpression)
        ]);

        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<JobExecutionLogger>();
        services.AddScoped<AbandonedCartJobHandler>();
        services.AddScoped<CartCleanupJobHandler>();
        services.AddScoped<OrderStatusJobHandler>();
        services.AddScoped<InventorySyncJobHandler>();
        services.AddScoped<IJobRunner, JobRunner>();

        return services;
    }

    public static IServiceCollection AddLumenQuartzScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        var jobOptions = configuration.GetSection(JobOptions.SectionName).Get<JobOptions>() ?? new JobOptions();

        if (!jobOptions.Enabled)
        {
            return services;
        }

        services.AddQuartz(q =>
        {
            RegisterJob<AbandonedCartQuartzJob>(q, LumenJobKeys.AbandonedCart, jobOptions.AbandonedCart.CronExpression);
            RegisterJob<CartCleanupQuartzJob>(q, LumenJobKeys.CartCleanup, jobOptions.CartCleanup.CronExpression);
            RegisterJob<OrderStatusQuartzJob>(q, LumenJobKeys.OrderStatus, jobOptions.OrderStatus.CronExpression);
            RegisterJob<InventorySyncQuartzJob>(q, LumenJobKeys.InventorySync, jobOptions.InventorySync.CronExpression);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }

    private static void RegisterJob<TJob>(
        IServiceCollectionQuartzConfigurator quartz,
        string jobKey,
        string cronExpression)
        where TJob : class, IJob
    {
        var key = new JobKey(jobKey);
        quartz.AddJob<TJob>(opts => opts.WithIdentity(key));
        quartz.AddTrigger(opts => opts
            .ForJob(key)
            .WithIdentity($"{jobKey}-trigger")
            .WithCronSchedule(cronExpression));
    }
}