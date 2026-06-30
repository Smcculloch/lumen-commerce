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
    }
}