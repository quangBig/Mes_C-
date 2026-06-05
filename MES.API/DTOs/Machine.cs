using System.ComponentModel.DataAnnotations;

namespace MES.API.DTOs;

/// <summary>Một lựa chọn trạng thái máy dùng cho select box ở frontend</summary>
public class MachineStatusOption
{
    public int    Id    { get; init; }
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

/// <summary>Các trạng thái hợp lệ của máy móc</summary>
public static class MachineStatus
{
    public const string Running     = "Running";     // Máy đang sản xuất
    public const string Idle        = "Idle";        // Máy sẵn sàng nhưng chưa có lệnh
    public const string Stop        = "Stop";        // Máy dừng sản xuất
    public const string Alarm       = "Alarm";       // Máy lỗi
    public const string Maintenance = "Maintenance"; // Máy bảo trì

    public static readonly string[] AllValues =
        [Running, Idle, Stop, Alarm, Maintenance];

    /// <summary>Danh sách đầy đủ dùng cho API trả về select box</summary>
    public static readonly List<MachineStatusOption> AllOptions =
    [
        new() { Id = 1, Value = Running,     Label = "Máy đang sản xuất" },
        new() { Id = 2, Value = Idle,        Label = "Máy sẵn sàng, chưa có lệnh sản xuất" },
        new() { Id = 3, Value = Stop,        Label = "Máy dừng sản xuất" },
        new() { Id = 4, Value = Alarm,       Label = "Máy lỗi" },
        new() { Id = 5, Value = Maintenance, Label = "Máy bảo trì" },
    ];
}

public class CreateMachineDto
{
    public int LineId { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    [AllowedValues("Running", "Idle", "Stop", "Alarm", "Maintenance",
        ErrorMessage = "Status phải là một trong: Running, Idle, Stop, Alarm, Maintenance.")]
    public string Status { get; set; } = MachineStatus.Idle;
}

public class UpdateMachineDto
{
    public int MachineId { get; set; }

    public int LineId { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    [AllowedValues("Running", "Idle", "Stop", "Alarm", "Maintenance",
        ErrorMessage = "Status phải là một trong: Running, Idle, Stop, Alarm, Maintenance.")]
    public string Status { get; set; } = MachineStatus.Idle;
}

public class MachineResponseDto
{
    public int MachineId { get; set; }

    public int LineId { get; set; }

    public string LineName { get; set; } = string.Empty;

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}