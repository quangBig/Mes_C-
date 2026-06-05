using System.ComponentModel.DataAnnotations.Schema;

namespace MES.API.Models;

public class WorkOrder
{
    public int      WorkOrderId   { get; set; }
    public string   WorkOrderCode { get; set; } = string.Empty;
    public int      ProductId     { get; set; }
    public int      Quantity      { get; set; }
    public DateTime StartTime     { get; set; }
    public DateTime EndTime       { get; set; }

    /// <summary>Pending | InProgress | Completed | Cancelled</summary>
    public string   Status        { get; set; } = WorkOrderStatus.Pending;

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    public ICollection<SerialNumber> SerialNumbers { get; set; } = [];
}

/// <summary>Các trạng thái hợp lệ của WorkOrder</summary>
public static class WorkOrderStatus
{
    public const string Pending    = "Pending";    // Chờ sản xuất
    public const string InProgress = "InProgress"; // Đang sản xuất
    public const string Completed  = "Completed";  // Hoàn thành
    public const string Cancelled  = "Cancelled";  // Đã hủy
}
