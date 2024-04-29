using Consul;

namespace BasketService.Api.Extensions;

public static class ConsulRegistration
{
    public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton<IConsulClient, ConsulClient>(sp => new ConsulClient(ConsulClientConfig =>
                                                                        {
                                                                            var address = configuration["ConsulConfig:Address"];
                                                                            ConsulClientConfig.Address = new Uri(address);
                                                                        }));
    }

    public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, IConfiguration configuration)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();

        var loggingFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
        var logger = loggingFactory.CreateLogger<IApplicationBuilder>();


        var uri = configuration.GetValue<Uri>("ConsulConfig:ServiceAddress");
        var serviceName = configuration.GetValue<string>("ConsulConfig:ServiceName");
        var serviceId = configuration.GetValue<string>("ConsulConfig:ServiceId");

        // Register service with Consul
        var registration = new AgentServiceRegistration()
        {
            ID = serviceId ?? "BasketService",
            Name = serviceName ?? "BasketService",
            Address = $"{uri.Host}",
            Port = uri.Port,
            Tags = [serviceName, serviceId]
        };

        logger.LogInformation("Registration with Consul");

        // Uygulama ayağa kalkarken daha önce Consul'a register edilmişse silip tekrar register ediyoruz.
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        consulClient.Agent.ServiceRegister(registration).Wait();

        // Uygulama sonlandırılırken Consul'dan siliyoruz.
        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Deregistering from Consul");
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });

        return app;
    }
}
