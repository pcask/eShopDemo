using CatalogService.Api.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CatalogService.Api.Infrastructure.Context;

public class CatalogContext(DbContextOptions<CatalogContext> options) : DbContext(options)
{
    public const string DEFAULT_SCHEMA = "catalog";

    public DbSet<CatalogType> CatalogTypes { get; set; }
    public DbSet<CatalogBrand> CatalogBrands { get; set; }
    public DbSet<CatalogItem> CatalogItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        //builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        //builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
