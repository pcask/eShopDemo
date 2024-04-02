using CatalogService.Api.Core.Domain.Entities.Common;

namespace CatalogService.Api.Core.Domain.Entities;

public class CatalogBrand : Entity<int> // Nike, Adidas etc.
{
    public string Brand { get; set; }
}