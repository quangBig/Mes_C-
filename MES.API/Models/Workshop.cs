namespace MES.API.Models;

public class Workshop
{
    public int WorkshopId { get; set; }

    public int FactoryId { get; set; }

    public string WorkshopCode { get; set; } = string.Empty;

    public string WorkshopName { get; set; } = string.Empty;

    public Factory? Factory { get; set; }
}