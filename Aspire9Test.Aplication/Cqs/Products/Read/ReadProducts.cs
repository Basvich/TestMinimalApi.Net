using Aspire9Test.Application.Domain;
using LiteBus.Queries.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire9Test.Application.Cqs.Products.Read {
  public class GetAll : IQuery<List<Product>> { }
  public class GetById : IQuery<Product?> {
    public int Id { get; set; }
  }

  //public class ReadProducts {
   

  //  public class ReadProductsHandler(IServiceProvider RequestServices) : ProductsHandlerBase(RequestServices),
  //    IQueryHandler<GetAll, List<Product>>, IQueryHandler<GetById, Product?>{
  //    public object Handle(object message) {
  //      if(message is GetById getById) {
  //        return HandleAsync(getById);
  //      } else if (message is GetAll getAll) {
  //        return HandleAsync(getAll);
  //      }
  //      return null;
  //    }

  //    public async Task<Product?> HandleAsync(GetById message, CancellationToken cancellationToken = default) {
  //      var res = await ProductsRepoService.GetProductById(message.Id);
  //      return res;
  //    }

  //    public async Task<List<Product>> HandleAsync(GetAll message, CancellationToken cancellationToken = default) {
  //      var res = await ProductsRepoService.GetAllProducts();
  //      return res;
  //    }
  //  }
  //}

  public class ReadProductsHandler(IServiceProvider RequestServices) : ProductsHandlerBase(RequestServices),
     IQueryHandler<GetById, Product?>, IQueryHandler<GetAll, List<Product>> {

    public object Handle(object message) {
      if (message is GetById getById) {
        return HandleAsync(getById);
      } else if (message is GetAll getAll) {
        return HandleAsync(getAll);
      }
      return null;
    }

    public async Task<Product?> HandleAsync(GetById message, CancellationToken cancellationToken = default) {
      var res = await ProductsRepoService.GetProductById(message.Id);
      return res;
    }

    public async Task<List<Product>> HandleAsync(GetAll message, CancellationToken cancellationToken = default) {
      var res = await ProductsRepoService.GetAllProducts();
      return res;
    }
  }
}
