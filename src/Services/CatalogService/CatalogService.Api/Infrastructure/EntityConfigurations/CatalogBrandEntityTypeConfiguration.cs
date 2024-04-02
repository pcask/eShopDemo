using CatalogService.Api.Core.Domain.Entities;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations;

public class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
{
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        builder.ToTable(nameof(CatalogBrand), CatalogContext.DEFAULT_SCHEMA);

        builder.HasKey(cb => cb.Id);

        builder.Property(cb => cb.Id)
            .UseHiLo("catalog_brand_hilo") // Kullanılan Db'e göre Id nin arttırımlı olarak oluşturulmasını bildiriyoruz. SQL server için Identity Specification
            .IsRequired();

        builder.Property(cb => cb.Brand)
            .IsRequired()
            .HasMaxLength(100);

    }
}