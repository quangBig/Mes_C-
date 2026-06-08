using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MES.API.Models;

public class ProductionTracking
{
    public long ProductionTrackingId { get; set; }
    public long SerialNumberId { get; set; }
    public int WorkOrderId { get; set; }
    public int? MachineId { get; set; }
    public int? OperatorId { get; set; }
    public string ProcessName { get; set; } = string.Empty; // Printer | SPI | Reflow | AOI | Packing
    public string Result { get; set; } = string.Empty;      // Pass | Fail
    public int? DefectId { get; set; }                      // Khóa ngoại trỏ sang Defects
    public string ScanSource { get; set; } = "Manual";      // Manual | Machine | Simulation
    public DateTime LoggedAt { get; set; } = DateTime.Now;

    [ForeignKey("SerialNumberId")]
    public SerialNumber? SerialNumber { get; set; }

    [ForeignKey("WorkOrderId")]
    public WorkOrder? WorkOrder { get; set; }

    [ForeignKey("MachineId")]
    public Machine? Machine { get; set; }

    [ForeignKey("OperatorId")]
    public User? Operator { get; set; }

    [ForeignKey("DefectId")]
    public Defect? Defect { get; set; }
}

public static class TrackingResult
{
    public const string Pass = "Pass";
    public const string Fail = "Fail";
}

public static class ScanSourceType
{
    public const string Manual = "Manual";
    public const string Machine = "Machine";
    public const string Simulation = "Simulation";
}

