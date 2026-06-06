using System;
using System.ComponentModel.DataAnnotations;

namespace MES.API.DTOs;

public class ScanSerialNumberDto
{
    [Required(ErrorMessage = "SerialCode là bắt buộc.")]
    public string SerialCode { get; set; } = string.Empty;

    public int? MachineId { get; set; }
    public int? OperatorId { get; set; }

    [Required(ErrorMessage = "ProcessName là bắt buộc.")]
    public string ProcessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Result là bắt buộc.")]
    [AllowedValues("Pass", "Fail", ErrorMessage = "Result phải là Pass hoặc Fail.")]
    public string Result { get; set; } = "Pass";

    public int? DefectId { get; set; }

    [AllowedValues("Manual", "Machine", "Simulation", ErrorMessage = "ScanSource phải là Manual, Machine hoặc Simulation.")]
    public string ScanSource { get; set; } = "Manual";
}

public class ProductionTrackingHistoryDto
{
    public string ProcessName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? DefectCode { get; set; }
    public string? DefectName { get; set; }
    public string? MachineName { get; set; }
    public string? OperatorName { get; set; }
    public string ScanSource { get; set; } = string.Empty;
    public DateTime LoggedAt { get; set; }
}
