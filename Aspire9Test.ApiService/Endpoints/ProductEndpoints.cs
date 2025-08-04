using Aspire9Test.Application.Cqs.Products.Cmd;
using Aspire9Test.Application.Cqs.Products.Read;
using Aspire9Test.Application.Domain;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Authentication;

namespace Aspire9Test.ApiService.Endpoints {




  public class ProductEndpoints: BaseEnpoints {

    
    public ProductEndpoints(IServiceProvider requestServices):base(requestServices) {      
    }

    public static void MapProductEndpoints(IEndpointRouteBuilder app) {
      var productsApi = app.MapGroup("/api/admin/products")
                           .WithTags("Products") // Opcional: para agrupar en Swagger UI
                           .WithOpenApi(); // Aplica a todo el grupo para Swagger

      productsApi.MapGet("/", (ProductEndpoints ep, CancellationToken ct) => ep.GetAllProducts(ct));
      productsApi.MapGet("/{id}", (ProductEndpoints ep, int id, CancellationToken ct) => ep.GetProductById(id, ct));
      productsApi.MapPost("/", (ProductEndpoints ep, Product product, CancellationToken ct) => ep.CreateProduct(product, ct));
      productsApi.MapPut("/{id}", (ProductEndpoints ep, int id, Product product, CancellationToken ct) => ep.UpdateProduct(id, product, ct));
      productsApi.MapDelete("/{id}", (ProductEndpoints ep, int id, CancellationToken ct) => ep.DeleteProduct(id, ct));
    }

    // Handlers de los endpoints (pueden ser métodos estáticos o instanciados vía DI)
    /// <summary>
    /// Obtiene la lista de todos los productos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de productos.</returns>
    private Task<IResult> GetAllProducts(CancellationToken ct = default) => GetMediatorIResult(new GetAll(), new OptMediatr { Aply404OnNull = false }, ct);

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El producto si existe, o NotFound si no existe.</returns>
    private Task<IResult> GetProductById(int id, CancellationToken ct = default) => GetMediatorIResult(new GetById { Id = id }, new OptMediatr { Aply404OnNull = true }, ct);

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="product">Datos del producto a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El producto creado.</returns>
    private async Task<IResult> CreateProduct(Product product, CancellationToken ct = default) {
      var created = await CommandMediator.SendAsync(new ModifyProducts.AddProduct { Product = product }, ct);
      //Puedo devolver created directamente, pero estoy probando la alternativa de  llamar otra ruta
      return Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="product">Datos actualizados del producto.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>NoContent si se actualizó, NotFound si no existe, BadRequest si el id no coincide.</returns>
    private async Task<IResult> UpdateProduct(int id, Product product, CancellationToken ct = default) {
      if (id != product.Id) return Results.BadRequest();
      var updated = await GetMediatorResult(new ModifyProducts.UpdateProduct { Product = product },  ct);      
      if (updated == null) return Results.NotFound();
      return Results.NoContent();
    }

    /// <summary>
    /// Elimina un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>NoContent tras eliminar el producto.</returns>
    private async Task<IResult> DeleteProduct(int id, CancellationToken ct = default) {
      var r=await GetMediatorResult(new ModifyProducts.DeleteProduct { ProductId = id }, ct);
      if(r) return Results.NoContent();
      return Results.NotFound();
    }

    /// <summary>
    /// Ejemplo alternativo de GetProductById usando GetMediatorIResult 
    /// para demostrar el manejo automático de errores y respuestas.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El producto si existe, o la respuesta HTTP apropiada.</returns>
    private async Task<IResult> GetProductByIdWithAutoHandling(int id, CancellationToken ct = default) {
      // Usando el nuevo método GetMediatorIResult con manejo automático de errores
      return await GetMediatorIResult(new GetById { Id = id }, new OptMediatr { Aply404OnNull = true }, ct);
    }
  }
}
