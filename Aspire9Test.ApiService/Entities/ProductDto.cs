namespace Aspire9Test.ApiService.Entities {

  /// <summary>
  /// Simple DTO for Product entity, en el que ocutamos el Discount property.
  /// </summary>
  public class ProductDto {
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
  }
}
