using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using PaymentService.Api.IntegrationEvents.EventHandlers;
using PaymentService.Api.IntegrationEvents.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OrderStartedIntegrationEventHandler içerisinde log bastığımız için;
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});

// Kullanılan eventBus'a bu ServiceProvidor gönderilecek ve ProcessEvent method'unda ServiceProvider içerisinden aşağıdaki handler'a ulaşılacaktır.
builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();

// Kullanılacak olan EventBus'ı IoC container'a ekliyoruz.
builder.Services.AddSingleton<IEventBus>(sp =>
{
    EventBusConfig config = new()
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "PaymentService",
        EventBusType = EventBusType.RabbitMQ
    };

    return EventBusFactory.Create(config, sp);
});

var app = builder.Build();

// Payment Service tarafından dinlenecek Event'lere, ilgili EventBus araçılığıyla Subscribe oluyoruz;
// Diğer Service'lerden EventBus'a "OrderStartedIntegrationEvent" Publish edildiğinde, bu event türüne subscribe olduğumuz için
// Consume işlemleri (Subscribe içerisinde Consume işlemini de tetikliyoruz şuan için) de gerçekleştirilecektir.
app.Services.GetRequiredService<IEventBus>()
            .Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();