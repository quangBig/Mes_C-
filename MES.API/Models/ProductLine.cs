using System.ComponentModel.DataAnnotations;

namespace MES.API.Models;

public class ProductionLine
{
    [Key]
    public int LineId { get; set; }

    public int WorkshopId { get; set; }

    public string LineCode { get; set; } = string.Empty;

    public string LineName { get; set; } = string.Empty;

    public Workshop? Workshop { get; set; }
}