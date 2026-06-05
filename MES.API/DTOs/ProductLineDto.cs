namespace MES.API.DTOs;

public class CreateProductionLineDto
{
    public int WorkshopId { get; set; }

    public string LineCode { get; set; } = string.Empty;

    public string LineName { get; set; } = string.Empty;
}

public class UpdateProductionLineDto
{
    public int WorkshopId { get; set; }

    public string LineCode { get; set; } = string.Empty;

    public string LineName { get; set; } = string.Empty;
}

public class ProductionLineResponseDto
{
    public int LineId { get; set; }

    public string LineCode { get; set; } = string.Empty;

    public string LineName { get; set; } = string.Empty;

    public int WorkshopId { get; set; }

    public string? WorkshopName { get; set; }
}