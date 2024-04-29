using IdentityService.Api.Application.Services;
using IdentityService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// Identity işlemlerimiz için yazdığımız service'i IoC container'a ekliyoruz.
builder.Services.AddScoped<IIdentityService, IdentityManager>();

// ConsulClient'ı IoC container'a ekleyelim.
builder.Services.ConfigureConsul(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Uygulamayı Consul'a register/deregister ediyoruz;
app.RegisterWithConsul(app.Lifetime, builder.Configuration);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
