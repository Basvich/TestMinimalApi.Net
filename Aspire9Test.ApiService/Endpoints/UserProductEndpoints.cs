using Aspire9Test.ApiService.Entities;
using Aspire9Test.Application.Cqs.Products.Read;
using Aspire9Test.Application.Domain;
using LiteBus.Queries.Abstractions;
using Mapster;
using MapsterMapper;
using System.Collections.Generic;

namespace Aspire9Test.ApiService.Endpoints {
  /// <summary>
  /// Enpoints sobre los productos, pero solo para consulta para los usuarios, con lo que solo tiene 2 funciones de lectura,
  /// de elmento individual y de todos, y ademas devuelve ProductDto.
  /// 
  /// Este ejemplo muestra ambas formas de manejar CancellationToken:
  /// 1. Como parámetro directo (recomendado)
  /// 2. Usando IHttpContextAccessor para casos especiales
  /// </summary>
  public class UserProductEndpoints: BaseEnpoints {    
    public UserProductEndpoints(IServiceProvider requestServices):base(requestServices) {      
    }

    public static void MapUserProductEndpoints(IEndpointRouteBuilder app) {
      var userProductsApi = app.MapGroup("/api/user/products")
                             .WithTags("User Products") // Opcional: para agrupar en Swagger UI
                             .WithOpenApi(); // Aplica a todo el grupo para Swagger

      userProductsApi.MapGet("/", (UserProductEndpoints ep, CancellationToken ct) => ep.GetAllProducts(ct));
      userProductsApi.MapGet("/{id}", (UserProductEndpoints ep, int id, CancellationToken ct) => ep.GetProductById(id, ct));
    }

    /// <summary>
    /// Obtiene la lista de todos los productos como ProductDto.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de productos como ProductDto.</returns>
    private Task<IResult> GetAllProducts(CancellationToken ct = default) => GetMappedMediatorIResult<List<Product>, List<ProductDto>>(new GetAll(), null, ct);

    /// <summary>
    /// Obtiene un producto por su identificador como ProductDto.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El producto como ProductDto si existe, o NotFound si no existe.</returns>
    private Task<IResult> GetProductById(int id, CancellationToken ct = default) => GetMappedMediatorIResult<Product?, ProductDto?>(new GetById { Id = id }, null, ct);

    /// <summary>
    /// Obtiene la lista de todos los productos como ProductDto.
    /// OPCIÓN 2: Usa IHttpContextAccessor para obtener el CancellationToken
    /// Esta opción es útil cuando el método no puede recibir el CancellationToken como parámetro
    /// </summary>
    /// <returns>Lista de productos como ProductDto.</returns>
    private async Task<IResult> GetAllProductsAlternative() {
      // Obtener el CancellationToken desde HttpContext
      var cancellationToken = HttpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
      
      var products = await ReadMediator.QueryAsync(new GetAll(), cancellationToken);
      var productDtos = products.Adapt<List<ProductDto>>();
      return Results.Ok(productDtos);
    }

    /// <summary>
    /// Método auxiliar que muestra cómo usar GetMediatorResult con HttpContextAccessor
    /// Equivalente al método en ProductEndpoints pero usando IHttpContextAccessor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    protected async Task<T> GetMediatorResultWithHttpContext<T>(IQuery<T> request) {
      // Obtener el CancellationToken desde HttpContext (equivalente a HttpContext.RequestAborted)
      var cancellationToken = HttpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
      
      async Task<T> act() => await ReadMediator.QueryAsync(request, cancellationToken);
      T? r;
      try {
        var reqStr = request.GetType().ToString();
        Logger.LogDebug("GetMediatorResultWithHttpContext() Sending request of type: \"{Req}\"", reqStr);
        r = await act();
      } catch (Exception ex) {
        ex.Data["nfo"] = "Handled";
        throw;
      }
      return r;
    }

 
  }
}
