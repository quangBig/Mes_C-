namespace MES.API.DTOs;

public class CreateFactoryDto
{
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
}

public class UpdateFactoryDto
{
    public int FactoryId { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
}
