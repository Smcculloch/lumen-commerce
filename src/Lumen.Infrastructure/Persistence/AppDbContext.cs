using Lumen.Infrastructure.Identity;
using Lumen.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Persistence;

/// <summary>
/// Primary database context for Lumen Commerce.
/// </summary>
public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TemplateDefinitionEntity> TemplateDefinitions => Set<TemplateDefinitionEntity>();
    public DbSet<PropertyDefinitionEntity> PropertyDefinitions => Set<PropertyDefinitionEntity>();
    public DbSet<ContentItemEntity> ContentItems => Set<ContentItemEntity>();
    public DbSet<MediaItemEntity> MediaItems => Set<MediaItemEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<ProductVariantEntity> ProductVariants => Set<ProductVariantEntity>();
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<CartEntity> Carts => Set<CartEntity>();
    public DbSet<CartItemEntity> CartItems => Set<CartItemEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderLineItemEntity> OrderLineItems => Set<OrderLineItemEntity>();
    public DbSet<OrderHistoryEntryEntity> OrderHistoryEntries => Set<OrderHistoryEntryEntity>();
    public DbSet<JobExecutionEntity> JobExecutions => Set<JobExecutionEntity>();
    public DbSet<NotificationLogEntity> NotificationLogs => Set<NotificationLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TemplateDefinitionEntity>(entity =>
        {
            entity.ToTable("TemplateDefinitions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.BaseTemplateKey).HasMaxLength(128);
            entity.HasMany(x => x.Properties)
                .WithOne(x => x.TemplateDefinition)
                .HasForeignKey(x => x.TemplateDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyDefinitionEntity>(entity =>
        {
            entity.ToTable("PropertyDefinitions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TemplateDefinitionId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ReferenceTemplateKey).HasMaxLength(128);
            entity.Property(x => x.Pattern).HasMaxLength(512);
        });

        modelBuilder.Entity<ContentItemEntity>(entity =>
        {
            entity.ToTable("ContentItems");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.FullPath).IsUnique();
            entity.HasIndex(x => x.MaterializedPath);
            entity.HasIndex(x => new { x.ParentId, x.SortOrder });
            entity.Property(x => x.TemplateKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(256).IsRequired();
            entity.Property(x => x.MaterializedPath).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.FullPath).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.PropertiesJson).IsRequired();
            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MediaItemEntity>(entity =>
        {
            entity.ToTable("MediaItems");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.FileName);
            entity.Property(x => x.FileName).HasMaxLength(512).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.PublicUrl).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AltText).HasMaxLength(512);
        });

        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.FullPath).IsUnique();
            entity.HasIndex(x => x.MaterializedPath);
            entity.HasIndex(x => new { x.ParentId, x.SortOrder });
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(256).IsRequired();
            entity.Property(x => x.MaterializedPath).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.FullPath).HasMaxLength(1024).IsRequired();
            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => new { x.CategoryId, x.SortOrder });
            entity.Property(x => x.TemplateKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Sku).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PropertiesJson).IsRequired();
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductVariantEntity>(entity =>
        {
            entity.ToTable("ProductVariants");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => new { x.ProductId, x.SortOrder });
            entity.Property(x => x.Sku).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PropertiesJson).IsRequired();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.Email);
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256);
        });

        modelBuilder.Entity<CartEntity>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SessionKey).IsUnique();
            entity.HasIndex(x => x.CustomerId);
            entity.Property(x => x.SessionKey).HasMaxLength(128);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Carts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItemEntity>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.CartId, x.ProductId, x.ProductVariantId });
            entity.Property(x => x.Sku).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProductName).HasMaxLength(256).IsRequired();
            entity.HasOne(x => x.Cart)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.PaymentStatus);
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.PaymentProvider).HasMaxLength(64);
            entity.Property(x => x.PaymentTransactionId).HasMaxLength(128);
            entity.Property(x => x.PaymentMessage).HasMaxLength(512);
            entity.Property(x => x.OrderNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CustomerName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ShippingName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ShippingLine1).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ShippingLine2).HasMaxLength(256);
            entity.Property(x => x.ShippingCity).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ShippingRegion).HasMaxLength(128);
            entity.Property(x => x.ShippingPostalCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ShippingCountry).HasMaxLength(128).IsRequired();
            entity.Property(x => x.BillingName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.BillingLine1).HasMaxLength(256).IsRequired();
            entity.Property(x => x.BillingLine2).HasMaxLength(256);
            entity.Property(x => x.BillingCity).HasMaxLength(128).IsRequired();
            entity.Property(x => x.BillingRegion).HasMaxLength(128);
            entity.Property(x => x.BillingPostalCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BillingCountry).HasMaxLength(128).IsRequired();
            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderLineItemEntity>(entity =>
        {
            entity.ToTable("OrderLineItems");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OrderId, x.ProductId, x.ProductVariantId });
            entity.Property(x => x.Sku).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProductName).HasMaxLength(256).IsRequired();
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderHistoryEntryEntity>(entity =>
        {
            entity.ToTable("OrderHistoryEntries");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OrderId, x.CreatedAt });
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Actor).HasMaxLength(256);
            entity.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobExecutionEntity>(entity =>
        {
            entity.ToTable("JobExecutions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.JobKey, x.StartedAt });
            entity.Property(x => x.JobKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2000);
        });

        modelBuilder.Entity<NotificationLogEntity>(entity =>
        {
            entity.ToTable("NotificationLogs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.Type).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Recipient).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        });
    }
}