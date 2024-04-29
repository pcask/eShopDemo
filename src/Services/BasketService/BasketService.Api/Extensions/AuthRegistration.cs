using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BasketService.Api.Extensions;

public static class AuthRegistration
{
    public static IServiceCollection ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthConfig:Secret"]!));

        services.AddAuthentication()
        .AddJwtBearer(opt =>
        {
            opt.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context => // Token validate sırasında bir hata alınması durumunda, buradan exception'ı inceleyebilirsin.
                {
                    return Task.CompletedTask;
                }
            };

            opt.TokenValidationParameters = new()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey
            };

        });

        return services;
    }
}
