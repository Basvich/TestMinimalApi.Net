using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire9Test.Application.Domain {
  public class Product {
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; } = (decimal)0.2;
    public int StockQuantity { get; set; } = 0;
  }
}
