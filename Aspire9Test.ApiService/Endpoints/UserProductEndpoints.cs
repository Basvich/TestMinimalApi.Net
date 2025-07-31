using Aspire9Test.ApiService.Entities;
using Aspire9Test.Application.Cqs.Products.Read;
using LiteBus.Queries.Abstractions;
using Mapster;

namespace Aspire9Test.ApiService.Endpoints {
  /// <summary>
  /// Enpoints sobre los productos, pero solo para consulta para los usuarios, con lo que solo tiene 2 funciones de lectura,
  /// de elmento individual y de todos, y ademas devuelve ProductDto.
  /// </summary>
  public class UserProductEndpoints {
    private readonly IServiceProvider requestServices;
    private IQueryMediator? _readMediator;
    protected IQueryMediator ReadMediator => _readMediator ??= requestServices.GetRequiredService<IQueryMediator>();

    public UserProductEndpoints(IServiceProvider requestServices) {
      this.requestServices = requestServices;
    }

    public static void MapUserProductEndpoints(IEndpointRouteBuilder app) {
      var userProductsApi = app.MapGroup("/api/user/products")
                             .WithTags("User Products") // Opcional: para agrupar en Swagger UI
                             .WithOpenApi(); // Aplica a todo el grupo para Swagger

      userProductsApi.MapGet("/", (UserProductEndpoints ep) => ep.GetAllProducts());
      userProductsApi.MapGet("/{id}", (UserProductEndpoints ep, int id) => ep.GetProductById(id));
    }

    /// <summary>
    /// Obtiene la lista de todos los productos como ProductDto.
    /// </summary>
    /// <returns>Lista de productos como ProductDto.</returns>
    private async Task<IResult> GetAllProducts() {
      var products = await ReadMediator.QueryAsync(new GetAll());
      var productDtos = products.Adapt<List<ProductDto>>();
      return Results.Ok(productDtos);
    }

    /// <summary>
    /// Obtiene un producto por su identificador como ProductDto.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <returns>El producto como ProductDto si existe, o NotFound si no existe.</returns>
    private async Task<IResult> GetProductById(int id) {
      var product = await ReadMediator.QueryAsync(new GetById { Id = id });
      if (product == null) return Results.NotFound();
      
      var productDto = product.Adapt<ProductDto>();
      return Results.Ok(productDto);
    }
  }
}
