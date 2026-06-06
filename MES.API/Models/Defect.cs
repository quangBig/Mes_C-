namespace MES.API.Models;

public class Defect
{
    public int DefectId { get; set; }
    public string DefectCode { get; set; } = string.Empty; // Ví dụ: DF001
    public string DefectName { get; set; } = string.Empty; // Ví dụ: Missing Component
}
