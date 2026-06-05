using System.ComponentModel.DataAnnotations;

namespace MES.API.Models;

public class Factory
{
    [Key]
    public int FactoryId { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
}
