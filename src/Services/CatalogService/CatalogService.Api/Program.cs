using CatalogService.Api.Extensions;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CatalogService.Api", Version = "v1" });
});

// Default'u "wwwroot" olan WebRootPath'i biz "Pics" klasörü olarak değiştiriyoruz. 
builder.Environment.WebRootPath = "Pics";

builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection("CatalogSettings"));

// DbContextRegistration için yazdığımız extension method;
builder.Services.ConfigureDbContext(builder.Configuration);

var app = builder.Build();


// Statik dosyalarımızın (ürün görselleri) tutulacağı klasörü belirtiyoruz.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Pics")),
    RequestPath = "/Pics"
});

// Database'in oluşturulması varsa uygulanmamış Migration'ların uygulanması için yazdığımız extension method;
await app.MigrateDbContext<CatalogContext>(async (context, services) =>
{
    var logger = services.GetService<ILogger<CatalogContextSeed>>()!;

    // Seeding işlemi için yazdığımız method;
    await new CatalogContextSeed().SeedAsync(context, builder.Environment, logger);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapControllers();
app.Run();


