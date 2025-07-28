using Aspire9Test.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire9Test.Application.Cqs.Products {
  public class ProductsHandlerBase : CqsHandlerBase {
    private ProductsRepoService? _productsRepoService;
    protected ProductsRepoService ProductsRepoService => _productsRepoService ??= RequestServices.GetRequiredService<ProductsRepoService>();

    public ProductsHandlerBase(IServiceProvider requestServices) : base(requestServices) {
    }
  }
}
