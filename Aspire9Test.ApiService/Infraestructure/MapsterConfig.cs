using Aspire9Test.ApiService.Entities;
using Aspire9Test.Application.Domain;
using Mapster;
using System.Reflection;

namespace Aspire9Test.ApiService.Infraestructure {
  public static class MapsterConfig {
    public static void RegisterMapsterConfiguration(this IServiceCollection services) {
      // mapster para pasar de Product a ProductDto
      TypeAdapterConfig<Product, ProductDto>
        .NewConfig()
        .Map(dest => dest.DiscountedPrice, src => src.Price*(1-src.Discount));
      TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
  
  }
}
