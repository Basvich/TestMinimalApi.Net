using Aspire9Test.Application.Domain;
using LiteBus.Commands.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire9Test.Application.Cqs.Products.Cmd {
  public class ModifyProducts {
    public class AddProduct : ICommand<Product> {
      public required Product Product { get; set; }
    }

    public class UpdateProduct : ICommand<Product?> {
      public required Product Product { get; set; }
    }

    public class DeleteProduct : ICommand<bool> {
      public required int ProductId { get; set; }
    }


    public class ModifyProductsHandler(IServiceProvider RequestServices) : ProductsHandlerBase(RequestServices),
      ICommandHandler<AddProduct, Product>, ICommandHandler<UpdateProduct, Product?>, ICommandHandler<DeleteProduct,bool> {
      public object Handle(object message) {
        throw new NotImplementedException();
      }

      public async Task<Product> HandleAsync(AddProduct message, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Product == null) {
          throw new ArgumentNullException(nameof(message), "Product cannot be null.");
        }
        if (string.IsNullOrWhiteSpace(message.Product.Name)) {
          throw new ArgumentException("Product name cannot be empty.", nameof(message));
        }
        if (message.Product.Price < 0) {
          throw new ArgumentOutOfRangeException(nameof(message), "Product.Price cannot be negative.");
        }
        var res = await ProductsRepoService.AddProduct(message.Product);
        return res;
      }

      public async Task<Product?> HandleAsync(UpdateProduct message, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(message);
        //Actualizar un producto existente, llamando a la funcion del servicio
        var res = await ProductsRepoService.UpdateProduct(message.Product);
        return res;
      }

      public Task<bool> HandleAsync(DeleteProduct message, CancellationToken cancellationToken = default) {
        var res = ProductsRepoService.DeleteProduct(message.ProductId);
        return res;
      }
    }
  }
}
