using Aspire9Test.ApiService.Endpoints;
using Aspire9Test.ApiService.Infraestructure;
using Aspire9Test.Application.Cqs;
using Aspire9Test.Application.Services;
using LiteBus.Commands.Extensions.MicrosoftDependencyInjection;
using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using LiteBus.Queries.Extensions.MicrosoftDependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

ServiceRegistration.RegisterServices(builder.Services);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment()) {
  app.MapOpenApi();
  app.MapScalarApiReference();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () => {
  var forecast = Enumerable.Range(1, 5).Select(index =>
      new WeatherForecast
      (
          DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
          Random.Shared.Next(-20, 55),
          summaries[Random.Shared.Next(summaries.Length)]
      ))
      .ToArray();
  return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

ProductEndpoints.MapProductEndpoints(app);
UserProductEndpoints.MapUserProductEndpoints(app);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary) {
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Mueve la declaración del método RegisterServices dentro de una clase para evitar el error CS8803 y CS0106
public static class ServiceRegistration {
  public static void RegisterServices(IServiceCollection services) {
    // Registrar LiteBus y sus componentes
    services.AddLiteBus(builder => {
      // Configurar queries
      builder.AddQueryModule(module => {
        // Registrar todos los query handlers de tu aplicación
        module.RegisterFromAssembly(typeof(CqsHandlerBase).Assembly);
      });
      builder.AddCommandModule(module => {
        // Registrar todos los query handlers de tu aplicación
        module.RegisterFromAssembly(typeof(CqsHandlerBase).Assembly);
      });
    });
    //El mock servicio/repositorio de productos
    services.AddScoped<ProductsRepoService>();
    services.AddScoped<ProductEndpoints>();
    services.AddScoped<UserProductEndpoints>();
    services.RegisterMapsterConfiguration();
  }
}
