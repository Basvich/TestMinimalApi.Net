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

  public class OptMediatr {
    /// <summary>return status 404 if result is null (not 204) </summary>
    public bool Aply404OnNull = true;
  }


  public class ProductEndpoints {

    private readonly IServiceProvider requestServices;
    private IQueryMediator? _readMediator;
    private IHttpContextAccessor? _httpContextAccessor;
    private ILogger<ProductEndpoints>? _logger;
    protected ILogger<ProductEndpoints> Logger => _logger ??= requestServices.GetRequiredService<ILogger<ProductEndpoints>>();
    protected IQueryMediator ReadMediator => _readMediator ??= requestServices.GetRequiredService<IQueryMediator>();
    private ICommandMediator? _commandMediator;
    protected ICommandMediator CommandMediator => _commandMediator ??= requestServices.GetRequiredService<ICommandMediator>();

    protected IHttpContextAccessor HttpContextAccessor =>
        _httpContextAccessor ??= requestServices.GetRequiredService<IHttpContextAccessor>();
    public ProductEndpoints(IServiceProvider requestServices) {
      this.requestServices = requestServices;
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
    private async Task<IResult> GetAllProducts(CancellationToken ct = default) {
      var products = await GetMediatorResult(new GetAll(), ct);
      return Results.Ok(products);
    }

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El producto si existe, o NotFound si no existe.</returns>
    private async Task<IResult> GetProductById(int id, CancellationToken ct = default) {
      // Usando el nuevo método GetMediatorIResult con manejo automático de errores
      return await GetMediatorIResult(new GetById { Id = id }, new OptMediatr { Aply404OnNull = true }, ct);
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="product">Datos del producto a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El producto creado.</returns>
    private async Task<IResult> CreateProduct(Product product, CancellationToken ct = default) {
      var created = await CommandMediator.SendAsync(new ModifyProducts.AddProduct { Product = product }, ct);
      //Puedo devolver created, pero estoy probando la alternativa de  llamar otra ruta
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

      var updated = await CommandMediator.SendAsync(new ModifyProducts.UpdateProduct { Product = product }, ct);
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
      await CommandMediator.SendAsync(new ModifyProducts.DeleteProduct { ProductId = id }, ct);
      return Results.NoContent();
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

    /// <summary>
    /// Get direct mediator result, without mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken">Token de cancelación para la operación.</param>
    /// <returns></returns>
    protected async Task<T> GetMediatorResult<T>(IQuery<T> request, CancellationToken cancellationToken = default) {
      async Task<T> act() => await ReadMediator.QueryAsync(request, cancellationToken);
      T? r;
      try {
        var reqStr = request.GetType().ToString();
        Logger.LogDebug("GetMediatorResult() Sending request of type: \"{Req}\"", reqStr);
        r = await act();
      } catch (Exception ex) {
        ex.Data["nfo"] = "Handled";
        throw;
      }
      return r;
    }

    /// <summary>
    /// Executes a mediator query and returns an IResult compatible with minimal APIs.
    /// Handles exceptions and converts them to appropriate HTTP status codes and problem details.
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="request">The query request to execute.</param>
    /// <param name="optr">Optional configuration for result handling.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An IResult representing the HTTP response.</returns>
    protected async Task<IResult> GetMediatorIResult<T>(IQuery<T> request, OptMediatr? optr = null, CancellationToken cancellationToken = default) {
      T? r;
      Exception? exeError = null;
      try {
        r = await GetMediatorResult<T>(request, cancellationToken);
      } catch (Exception ex) {
        exeError = ex;
        r = default;
      }

      var statusCode = ExceptionToStatusCode(exeError);
      
      if (exeError == null) {
        // Handle successful execution
        if (r == null && optr?.Aply404OnNull == true) {
          return Results.NotFound();
        }
        if (r == null) {
          return Results.NoContent();
        }
        return Results.Ok(r);
      } else {
        // Handle exception
        var respError = ExceptionToProblem(exeError);
        return Results.Problem(
          detail: respError?.Detail,
          statusCode: statusCode,
          title: respError?.Type,
          type: respError?.Type,
          extensions: respError?.Extensions
        );
      }
    }

    /// <summary>
    /// Legacy method for MVC controllers - returns ActionResult<T>
    /// </summary>
    protected async Task<ActionResult<T>> GetMediatorActionResult<T>(IQuery<T> request, OptMediatr? optr = null) {
      T? r;
      ObjectResult res;
      Exception? exeError = null;
      try {
        r = await GetMediatorResult<T>(request);
        if (r == null) return new NoContentResult(); 
      } catch (Exception ex) {
        exeError = ex;
        r = default;
      }
      var statusCode = ExceptionToStatusCode(exeError);
      if (exeError == null) {
        if (r == null && optr?.Aply404OnNull == true) statusCode = 404;
        res = new ObjectResult(r) {
          StatusCode = statusCode
        };
      } else {
        var respError = ExceptionToProblem(exeError);
        res = new ObjectResult(respError) {
          StatusCode = statusCode
        };
      }
      return res;
    }

    protected int ExceptionToStatusCode(Exception? e) {
      if (e == null) return StatusCodes.Status200OK;
      var res = e switch {
        KeyNotFoundException => StatusCodes.Status404NotFound,
        NotImplementedException => StatusCodes.Status501NotImplemented,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        InvalidCredentialException => StatusCodes.Status401Unauthorized,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest,
      };
      return res;
    }

    protected ProblemDetails? ExceptionToProblem(Exception? e) {
      if (e == null) return null;
      ProblemDetails? res = null;
      if (e is FluentValidation.ValidationException vex) {
        var r = new ValidationProblemDetails();
        foreach (var ee in vex.Errors) {
          r.Errors.TryAdd(ee.PropertyName, [ee.ErrorCode, ee.ErrorMessage]);
        }
        r.Status = StatusCodes.Status400BadRequest;
        res = r;
        return res;
      }
      res = new ProblemDetails {
        Type = e.GetType().Name,
        Detail = e.Message,
        Status = ExceptionToStatusCode(e)
      };
      return res;
    }
  }
}
