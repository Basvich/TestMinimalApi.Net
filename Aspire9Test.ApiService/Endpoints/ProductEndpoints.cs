using Aspire9Test.Application.Cqs.Products.Cmd;
using Aspire9Test.Application.Cqs.Products.Read;
using Aspire9Test.Application.Domain;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;

namespace Aspire9Test.ApiService.Endpoints {
  public  class ProductEndpoints {

    private readonly IServiceProvider requestServices;
    private IQueryMediator? _readMediator;
    protected IQueryMediator ReadMediator => _readMediator ??= requestServices.GetRequiredService<IQueryMediator>();
    private ICommandMediator? _commandMediator;
    protected ICommandMediator CommandMediator => _commandMediator ??= requestServices.GetRequiredService<ICommandMediator>();
    public ProductEndpoints(IServiceProvider requestServices) {
      this.requestServices = requestServices;
    }

    public static void MapProductEndpoints(IEndpointRouteBuilder app) {
      var productsApi = app.MapGroup("/api/admin/products")
                           .WithTags("Products") // Opcional: para agrupar en Swagger UI
                           .WithOpenApi(); // Aplica a todo el grupo para Swagger

      productsApi.MapGet("/", (ProductEndpoints ep)  => ep.GetAllProducts());
      productsApi.MapGet("/{id}", (ProductEndpoints ep, int id) => ep.GetProductById(id));
      productsApi.MapPost("/", (ProductEndpoints ep, Product product) => ep.CreateProduct(product));
      productsApi.MapPut("/{id}", (ProductEndpoints ep, int id, Product product) => ep.UpdateProduct(id, product));
      productsApi.MapDelete("/{id}", (ProductEndpoints ep, int id) => ep.DeleteProduct(id));
    }

    // Handlers de los endpoints (pueden ser métodos estáticos o instanciados vía DI)
    /// <summary>
    /// Obtiene la lista de todos los productos.
    /// </summary>
    /// <param name="mediator">Mediador de consultas.</param>
    /// <returns>Lista de productos.</returns>
    private async Task<IResult> GetAllProducts() {
      var products = await ReadMediator.QueryAsync(new GetAll());
      return Results.Ok(products);
    }

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="mediator">Mediador de consultas.</param>
    /// <returns>El producto si existe, o NotFound si no existe.</returns>
    private async Task<IResult> GetProductById(int id) {
      var product = await ReadMediator.QueryAsync(new GetById { Id = id });
      return product != null ? Results.Ok(product) : Results.NotFound();
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="product">Datos del producto a crear.</param>
    /// <param name="mediator">Mediador de comandos.</param>
    /// <returns>El producto creado.</returns>
    private async Task<IResult> CreateProduct(Product product) {
      var created = await CommandMediator.SendAsync(new ModifyProducts.AddProduct { Product =product});
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
    private async Task<IResult> UpdateProduct(int id, Product product) {
      if (id != product.Id) return Results.BadRequest();     

      var updated= await CommandMediator.SendAsync(new ModifyProducts.UpdateProduct { Product = product });
      if (updated == null) return Results.NotFound();
      return Results.NoContent();
    }

    /// <summary>
    /// Elimina un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <param name="mediator">Mediador de comandos.</param>
    /// <returns>NoContent tras eliminar el producto.</returns>
    private async Task<IResult> DeleteProduct(int id) {
      await CommandMediator.SendAsync(new ModifyProducts.DeleteProduct {ProductId=id });
      return Results.NoContent();
    }
  }
}
