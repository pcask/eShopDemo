using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Extensions;
using BasketService.Api.Infrastructure.Repository;
using BasketService.Api.IntegrationEvents.EventHandlers;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace BasketService.Api;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterBasketApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                                               {
                                                   { jwtSecurityScheme, Array.Empty<string>() }
                                               });
        });

        // BasketService kendi pushladığı OrderCreatedIntegrationEvent'i dinleyecek ve ardından müşteri basket'ı temizlenecektir.
        services.AddTransient<OrderCreatedIntegrationEventHandler>();

        // Kullanılacak olan EventBus'ı IoC container'a ekliyoruz.
        services.AddSingleton<IEventBus>(sp =>
        {
            EventBusConfig config = new()
            {
                ConnectionRetryCount = 5,
                EventNameSuffix = "IntegrationEvent",
                SubscriberClientAppName = "BasketService",
                EventBusType = EventBusType.RabbitMQ
            };

            return EventBusFactory.Create(config, sp);
        });

        // Jwt ile Authentication işlemleri için;
        services.ConfigureAuth(configuration);

        // ConsulClient'i IoC container'a ekliyoruz.
        services.ConfigureConsul(configuration);

        // Singleton olarak Redis connection'ı (ConnectionMultiplexer) IoC container'a ekliyoruz.
        services.AddSingleton(sp => sp.ConfigureRedis(configuration));

        services.AddHttpContextAccessor();

        services.AddScoped<IBasketRepository, RedisBasketRepository>();
        services.AddTransient<IIdentityService, IdentityManager>();

        return services;
    }
}
