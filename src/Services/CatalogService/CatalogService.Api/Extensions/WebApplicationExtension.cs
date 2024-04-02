using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace CatalogService.Api.Extensions;

public static class WebApplicationExtension
{
    public static async Task<WebApplication> MigrateDbContext<TContext>(this WebApplication webApp, Func<TContext, IServiceProvider, Task> seeder)
            where TContext : DbContext
    {
        // Using statement'i içerisindeki oluşturulan tüm nesnelerin, retry mekanizması ile execute edilen işlem bittikten sonra dispose edilmesini sağlamak için
        // retry.ExecuteAsync() methodunu kullanmamız ve await ile beklememiz gerekiyor! Aksi takdirde gönderdiğimiz nesneler daha kullanımladan dispose edilecektir!
        using (var scope = webApp.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILogger<TContext>>();

            var context = services.GetService<TContext>();

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                var retry = Policy.Handle<SqlException>()
                         .WaitAndRetryAsync(
                         [
                                 TimeSpan.FromSeconds(3),
                                 TimeSpan.FromSeconds(5),
                                 TimeSpan.FromSeconds(8),
                         ]);

                await retry.ExecuteAsync(async () => await InvokeSeeder(seeder, context, services));

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
            }
        }

        return webApp;
    }

    private static async Task InvokeSeeder<TContext>(Func<TContext, IServiceProvider, Task> seeder, TContext context, IServiceProvider services)
        where TContext : DbContext
    {
        context.Database.EnsureCreated();
        context.Database.Migrate();
        await seeder(context, services);
    }
}
