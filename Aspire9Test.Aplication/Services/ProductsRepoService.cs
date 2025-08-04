using Aspire9Test.Application.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire9Test.Application.Services {
  public class ProductsRepoService {
    private readonly List<Product> _products =
   [
        new() { Id = 1, Name = "Laptop", Price = 1200m },
        new() { Id = 2, Name = "Mouse", Price = 25m }
    ];

    public Task<List<Product>> GetAllProducts() => Task.FromResult(_products);
    public Task<Product?> GetProductById(int id) => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    public Task<Product> AddProduct(Product product) {
      product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
      _products.Add(product);
      return Task.FromResult( product);
    }

    public Task<Product?> UpdateProduct(Product product) {
      var existing = _products.FirstOrDefault(p => p.Id == product.Id);
      if (existing != null) {
        existing.Name = product.Name;
        existing.Price = product.Price;
      } 
      return Task.FromResult(existing);
    }

    public Task<bool> DeleteProduct(int id) {
      var c=_products.RemoveAll(p => p.Id == id);
      return Task.FromResult(c > 0);
    }
  }
}
