using System.ComponentModel.DataAnnotations;

namespace MES.API.DTOs;

// ─── WorkOrder Status Options ────────────────────────────────────────────────

public class WorkOrderStatusOption
{
    public int    Id    { get; init; }
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

public static class WorkOrderStatusList
{
    public static readonly List<WorkOrderStatusOption> AllOptions =
    [
        new() { Id = 1, Value = "Pending",    Label = "Chờ sản xuất"  },
        new() { Id = 2, Value = "InProgress", Label = "Đang sản xuất" },
        new() { Id = 3, Value = "Completed",  Label = "Hoàn thành"    },
        new() { Id = 4, Value = "Cancelled",  Label = "Đã hủy"        },
    ];
}

// ─── Create ──────────────────────────────────────────────────────────────────

public class CreateWorkOrderDto
{
    [Required(ErrorMessage = "WorkOrderCode là bắt buộc.")]
    public string WorkOrderCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "ProductId là bắt buộc.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity phải lớn hơn 0.")]
    public int Quantity { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime   { get; set; }
}

// ─── Update ──────────────────────────────────────────────────────────────────

/// <summary>
/// Chỉ cho phép sửa: WorkOrderCode, StartTime, EndTime, Status.
/// ProductId và Quantity bị khóa sau khi đã sinh Serial Numbers.
/// </summary>
public class UpdateWorkOrderDto
{
    [Required(ErrorMessage = "WorkOrderCode là bắt buộc.")]
    public string WorkOrderCode { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime   { get; set; }

    [AllowedValues("Pending", "InProgress", "Completed", "Cancelled",
        ErrorMessage = "Status phải là: Pending, InProgress, Completed hoặc Cancelled.")]
    public string Status { get; set; } = "Pending";
}

// ─── Response ─────────────────────────────────────────────────────────────────

public class SerialNumberDto
{
    public long   SerialNumberId { get; set; }
    public string SerialCode     { get; set; } = string.Empty;
    public string Status         { get; set; } = string.Empty;
}

public class WorkOrderResponseDto
{
    public int    WorkOrderId   { get; set; }
    public string WorkOrderCode { get; set; } = string.Empty;
    public int    ProductId     { get; set; }
    public string ProductCode   { get; set; } = string.Empty;
    public string ProductName   { get; set; } = string.Empty;
    public int    Quantity      { get; set; }
    public DateTime StartTime   { get; set; }
    public DateTime EndTime     { get; set; }
    public string Status        { get; set; } = string.Empty;
    public List<SerialNumberDto> SerialNumbers { get; set; } = [];
}
