using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using LiteBus.Queries.Extensions.MicrosoftDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Aspire9Test.Tests {
  public static class TestSetup {
    public static IServiceProvider ConfigureTestServices() {
      var services = new ServiceCollection();

      // Registrar LiteBus y sus componentes
      services.AddLiteBus(builder => {
        // Configurar queries
        builder.AddQueryModule(module => {
          // Registrar todos los query handlers de tu aplicación
          module.RegisterFromAssembly(typeof(Aspire9Test.Aplication.Class1).Assembly);
        });
        //builder.AddQueryModule(module =>
        //{
        //  module.RegisterFromAssembly(typeof(GetProductQuery).Assembly);
        //});

        //builder.AddEventModule(module =>
        //{
        //  module.RegisterFromAssembly(typeof(ProductCreatedEvent).Assembly);
        //});
      });

      // Aquí puedes agregar más servicios que necesites para tus pruebas

      return services.BuildServiceProvider();
    }
  }
}