using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using Microsoft.Extensions.Hosting;
using LiteBus.Queries.Extensions.MicrosoftDependencyInjection;

namespace Aspire9Test.Tests {
  internal class LiteBusFixture {
    public IServiceProvider ServiceProvider { get; }

    public LiteBusFixture() {
      var builder = Host.CreateDefaultBuilder()
          .ConfigureServices(services => {
            services.AddLiteBus(liteBus =>
            {
              liteBus.AddQueryModule(module => {
                // Registrar todos los query handlers de tu aplicación
                module.RegisterFromAssembly(typeof(Aspire9Test.Aplication.Class1).Assembly);
              });
            });
          });

      var host = builder.Build();
      ServiceProvider = host.Services;
    }
  }
}
