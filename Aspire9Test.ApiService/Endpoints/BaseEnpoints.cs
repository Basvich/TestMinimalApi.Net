using Aspire9Test.ApiService.Entities;
using Aspire9Test.Application.Domain;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace Aspire9Test.ApiService.Endpoints {
  public class BaseEnpoints {
    private readonly IServiceProvider requestServices;
    private IQueryMediator? _readMediator;
    private IHttpContextAccessor? _httpContextAccessor;
    private ILogger<BaseEnpoints>? _logger;
    protected ILogger<BaseEnpoints> Logger => _logger ??= requestServices.GetRequiredService<ILogger<BaseEnpoints>>();
    protected IQueryMediator ReadMediator => _readMediator ??= requestServices.GetRequiredService<IQueryMediator>();
    private ICommandMediator? _commandMediator;
    protected ICommandMediator CommandMediator => _commandMediator ??= requestServices.GetRequiredService<ICommandMediator>();

    protected IHttpContextAccessor HttpContextAccessor =>
        _httpContextAccessor ??= requestServices.GetRequiredService<IHttpContextAccessor>();
    public BaseEnpoints(IServiceProvider requestServices) {
      this.requestServices = requestServices;
    }

    /// <summary>
    /// Get direct mediator result, without mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken">Token de cancelación para la operación.</param>
    /// <returns></returns>
    protected async Task<T> GetMediatorResult<T>(IQuery<T> request, CancellationToken cancellationToken) {
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

    protected async Task<T> GetMediatorResult<T>(ICommand<T> request, CancellationToken cancellationToken) {
      async Task<T> act() => await CommandMediator.SendAsync(request, cancellationToken);
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
    /// Executes the specified query using the mediator, maps the result to the specified type, and returns the mapped
    /// result.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the mediator.</typeparam>
    /// <typeparam name="TMapped">The type to which the mediator result will be mapped.</typeparam>
    /// <param name="request">The query to be executed by the mediator.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the mapped result of type
    /// <typeparamref name="TMapped"/>.</returns>
    protected async Task<TMapped> GetMappedMediatorResult<T, TMapped>(IQuery<T> request, CancellationToken cancellationToken) {
      T r1 = await GetMediatorResult(request, cancellationToken);
      var res=r1.Adapt<TMapped>();      
      return res;
    }

    protected async Task<TMapped> GetMappedMediatorResult<T, TMapped>(ICommand<T> request, CancellationToken cancellationToken) {
      T r1 = await GetMediatorResult(request, cancellationToken);
      var res = r1.Adapt<TMapped>();
      return res;
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

    protected async Task<IResult> GetMediatorIResult<T>(ICommand<T> request, OptMediatr? optr = null, CancellationToken cancellationToken = default) {
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
    /// Return direct an object of type to acction result, based on mediator command, and a mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TR"></typeparam>
    /// <param name="request"></param>
    /// <param name="optr"></param>
    /// <returns></returns>
    protected async Task<IResult> GetMappedMediatorIResult<T, TR>(IQuery<T> request, OptMediatr? optr = null, CancellationToken cancellationToken = default) {      
      Exception? exeError = null;
      TR? r;
      try {
        r = await GetMappedMediatorResult<T, TR>(request, cancellationToken);
      } catch (Exception ex) {
        exeError = ex;
        r = default;
      }
      var statusCode = ExceptionToStatusCode(exeError);
      if (exeError == null) {
        if (r == null && optr?.Aply404OnNull == true) statusCode = 404;
        if (statusCode == StatusCodes.Status200OK) return Results.Ok(r);
        return Results.StatusCode(statusCode);        
      } else {
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
    protected async Task<ActionResult<T>> GetMediatorActionResult<T>(IQuery<T> request, OptMediatr? optr = null, CancellationToken cancellationToken= default) {
      T? r;
      ObjectResult res;
      Exception? exeError = null;
      try {
        r = await GetMediatorResult<T>(request, cancellationToken);
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
