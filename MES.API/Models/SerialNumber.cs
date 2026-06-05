using System.ComponentModel.DataAnnotations.Schema;

namespace MES.API.Models;

/// <summary>Các trạng thái hợp lệ của Serial Number</summary>
public static class SerialStatus
{
    public const string Pending   = "Pending";   // Chưa sản xuất
    public const string Pass      = "Pass";      // Đạt
    public const string Fail      = "Fail";      // Lỗi
    public const string Cancelled = "Cancelled"; // Bị hủy theo WorkOrder
}

public class SerialNumber
{
    public long  SerialNumberId { get; set; }
    public int    WorkOrderId    { get; set; }
    public string SerialCode     { get; set; } = string.Empty;
    public string Status         { get; set; } = SerialStatus.Pending;

    [ForeignKey("WorkOrderId")]
    public WorkOrder? WorkOrder { get; set; }
}
