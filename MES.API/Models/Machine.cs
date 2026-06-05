using System.ComponentModel.DataAnnotations.Schema;

namespace MES.API.Models;

public class Machine
{
    public int MachineId { get; set; }
    public int LineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    [ForeignKey("LineId")]
    public ProductionLine? ProductionLine { get; set; }
}
