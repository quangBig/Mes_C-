using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MES.API.Models;

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
    [AllowedValues(TrackingResult.Pass, TrackingResult.Fail, ErrorMessage = "Result phải là Pass hoặc Fail.")]
    public string Result { get; set; } = TrackingResult.Pass;

    public int? DefectId { get; set; }

    [AllowedValues(ScanSourceType.Manual, ScanSourceType.Machine, ScanSourceType.Simulation, ErrorMessage = "ScanSource phải là Manual, Machine hoặc Simulation.")]
    public string ScanSource { get; set; } = ScanSourceType.Manual;
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

public class SimulateStepDto
{
    [Required(ErrorMessage = "MachineId là bắt buộc.")]
    public int MachineId { get; set; }
}

public class SimulateDto
{
    [Required(ErrorMessage = "WorkOrderId là bắt buộc.")]
    public int WorkOrderId { get; set; }

    [Range(0, 100, ErrorMessage = "Tỷ lệ lỗi defectRate phải từ 0 đến 100.")]
    public double DefectRate { get; set; } = 0;

    [Required(ErrorMessage = "OperatorId là bắt buộc.")]
    public int OperatorId { get; set; }

    [Range(1, 60, ErrorMessage = "Khoảng thời gian quét (IntervalMinutes) phải từ 1 đến 60 phút.")]
    public int IntervalMinutes { get; set; } = 2;

    [Required(ErrorMessage = "Danh sách công đoạn steps là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Phải cung cấp ít nhất một công đoạn quét.")]
    public List<SimulateStepDto> Steps { get; set; } = [];
}

public class SimulateResponseDto
{
    public int WorkOrderId { get; set; }
    public int TotalSerials { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public double Yield { get; set; }
}
