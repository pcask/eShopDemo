using CatalogService.Api.Core.Domain.Entities.Common;

namespace CatalogService.Api.Core.Domain.Entities;

public class CatalogType : Entity<int> // T-shit, Shoes, Pants, etc.
{
    public string Type { get; set; }
}
