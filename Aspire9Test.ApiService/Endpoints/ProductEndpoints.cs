using Aspire9Test.Application.Cqs.Products.Cmd;
using Aspire9Test.Application.Cqs.Products.Read;
using Aspire9Test.Application.Domain;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;

namespace Aspire9Test.ApiService.Endpoints {
  public static  class ProductEndpoints {
    public static void MapProductEndpoints(this IEndpointRouteBuilder app) {
      var productsApi = app.MapGroup("/api/products")
                           .WithTags("Products") // Opcional: para agrupar en Swagger UI
                           .WithOpenApi(); // Aplica a todo el grupo para Swagger

      productsApi.MapGet("/", GetAllProducts);
      productsApi.MapGet("/{id}", GetProductById);
      productsApi.MapPost("/", CreateProduct);
      productsApi.MapPut("/{id}", UpdateProduct);
      productsApi.MapDelete("/{id}", DeleteProduct);
    }

    // Handlers de los endpoints (pueden ser métodos estáticos o instanciados vía DI)
    /// <summary>
    /// Obtiene la lista de todos los productos.
    /// </summary>
    /// <param name="mediator">Mediador de consultas.</param>
    /// <returns>Lista de productos.</returns>
    private static async Task<IResult> GetAllProducts(IQueryMediator mediator) {
      var products = await mediator.QueryAsync(new GetAll());
      return Results.Ok(products);
    }

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="mediator">Mediador de consultas.</param>
    /// <returns>El producto si existe, o NotFound si no existe.</returns>
    private static async Task<IResult> GetProductById(int id, IQueryMediator mediator) {
      var product = await mediator.QueryAsync(new GetById { Id = id });
      return product != null ? Results.Ok(product) : Results.NotFound();
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="product">Datos del producto a crear.</param>
    /// <param name="mediator">Mediador de comandos.</param>
    /// <returns>El producto creado.</returns>
    private static async Task<IResult> CreateProduct(Product product, ICommandMediator mediator) {
      var created = await mediator.SendAsync(new ModifyProducts.AddProduct { Product =product});
      //Puedo devolver created, pero estoy probando la alternativa de  llamar otra ruta
      return Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="product">Datos actualizados del producto.</param>
    /// <param name="mediator">Mediador de comandos.</param>
    /// <returns>NoContent si se actualizó, NotFound si no existe, BadRequest si el id no coincide.</returns>
    private static async Task<IResult> UpdateProduct(int id, Product product, ICommandMediator mediator) {
      if (id != product.Id) return Results.BadRequest();     

      var updated= await mediator.SendAsync(new ModifyProducts.UpdateProduct { Product = product });
      if (updated == null) return Results.NotFound();
      return Results.NoContent();
    }

    /// <summary>
    /// Elimina un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <param name="mediator">Mediador de comandos.</param>
    /// <returns>NoContent tras eliminar el producto.</returns>
    private static async Task<IResult> DeleteProduct(int id, ICommandMediator mediator) {
      await mediator.SendAsync(new ModifyProducts.DeleteProduct {ProductId=id });
      return Results.NoContent();
    }
  }
}
