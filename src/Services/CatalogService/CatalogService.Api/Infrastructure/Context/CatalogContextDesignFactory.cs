using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Infrastructure.Context;

// Design-Time'da yani henüz uygulamamız çalışMıyorken, diğer bir değişle henüz geliştirme yapıyorken, Migrations veya Scaffold gibi işlemleri yapabilmemiz için;
public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
{
    public CatalogContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDemo;Integrated Security=True;");

        return new CatalogContext(optionsBuilder.Options);
    }
}
