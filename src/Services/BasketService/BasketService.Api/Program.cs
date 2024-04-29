using BasketService.Api;
using BasketService.Api.Extensions;
using BasketService.Api.IntegrationEvents.EventHandlers;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

builder.Services.RegisterBasketApiServices(builder.Configuration);


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// BasketService kendi pushladığı OrderCreatedIntegrationEvent'i dinleyecek ve ardından müşteri basket'ı temizlenecektir.
app.Services.GetRequiredService<IEventBus>()
            .Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

// BasketService' imizi Consul'a ekliyoruz.
app.RegisterWithConsul(app.Lifetime, builder.Configuration);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();