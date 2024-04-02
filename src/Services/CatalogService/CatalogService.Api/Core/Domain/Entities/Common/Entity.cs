namespace CatalogService.Api.Core.Domain.Entities.Common
{
    public class Entity
    {
    }

    public class Entity<TKey> : Entity
    {
        public TKey Id { get; set; }
    }
}
