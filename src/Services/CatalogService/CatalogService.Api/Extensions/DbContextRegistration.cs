using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CatalogService.Api.Extensions;

public static class DbContextRegistration
{
    public static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEntityFrameworkSqlServer()
            .AddDbContext<CatalogContext>(options =>
            {
                options.UseSqlServer(configuration["ConnectionString"],
                                     sqlServerOptionsAction: sqlOptions =>
                                     {
                                         // EF Core, belirtilen assembly içerisindeki migration'ları ele alacak.
                                         sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                     });
            });

        return services;
    }
}
