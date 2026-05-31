namespace MES.API.DTOs;

public class CreateWorkshopDto
{
    public int FactoryId { get; set; }
    public string WorkshopCode { get; set; } = string.Empty;
    public string WorkshopName { get; set; } = string.Empty;
}

public class UpdateWorkshopDto
{
    public int WorkshopId { get; set; }
    public int FactoryId { get; set; }
    public string WorkshopCode { get; set; } = string.Empty;
    public string WorkshopName { get; set; } = string.Empty;
}
