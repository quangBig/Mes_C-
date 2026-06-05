using System.ComponentModel.DataAnnotations;

namespace MES.API.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "ProductCode là bắt buộc.")]
    public string ProductCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "ProductName là bắt buộc.")]
    public string ProductName { get; set; } = string.Empty;
}

public class UpdateProductDto
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "ProductCode là bắt buộc.")]
    public string ProductCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "ProductName là bắt buộc.")]
    public string ProductName { get; set; } = string.Empty;
}

public class ProductResponseDto
{
    public int    ProductId   { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}
