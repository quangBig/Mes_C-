using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MES.API.Data;
using MES.API.DTOs;
using MES.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MES.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductionTrackingController : ControllerBase
{
    private readonly MESDbContext _context;

    public ProductionTrackingController(MESDbContext context)
    {
        _context = context;
    }

    // POST: api/ProductionTracking/scan
    // ✅ Ghi nhận quét mã Serial Number tại công đoạn sản xuất
    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] ScanSerialNumberDto dto)
    {
        // 1. Tìm SerialNumber theo mã SerialCode
        var serial = await _context.SerialNumbers
            .FirstOrDefaultAsync(s => s.SerialCode == dto.SerialCode);

        if (serial == null)
            return NotFound(new { message = $"Mã Serial '{dto.SerialCode}' không tồn tại." });

        // 2. Chống quét trùng tại cùng một công đoạn cho Serial đó
        var existed = await _context.ProductionTrackings
            .AnyAsync(x => x.SerialNumberId == serial.SerialNumberId && 
                           x.ProcessName == dto.ProcessName);

        if (existed)
            return BadRequest(new { message = $"Serial '{dto.SerialCode}' đã được quét tại công đoạn '{dto.ProcessName}' trước đó." });

        // 3. Kiểm tra tính hợp lệ và trạng thái hoạt động của máy (Machine)
        if (dto.MachineId.HasValue)
        {
            var machine = await _context.Machines.FindAsync(dto.MachineId.Value);
            if (machine == null)
            {
                return BadRequest(new { message = $"Máy với Id {dto.MachineId.Value} không tồn tại." });
            }

            // Máy ở trạng thái không sẵn sàng (Alarm, Maintenance, Stop) sẽ không cho phép quét
            if (machine.Status == "Alarm" || machine.Status == "Maintenance" || machine.Status == "Stop")
            {
                return BadRequest(new { message = $"Máy '{machine.MachineName}' ({machine.MachineCode}) đang ở trạng thái '{machine.Status}', không thể tiến hành quét sản xuất." });
            }
        }

        // 4. Kiểm tra tính hợp lệ của DefectId nếu Result là Fail
        if (dto.Result == "Fail" && dto.DefectId.HasValue)
        {
            var defectExists = await _context.Defects.AnyAsync(d => d.DefectId == dto.DefectId.Value);
            if (!defectExists)
                return BadRequest(new { message = $"Mã lỗi DefectId {dto.DefectId.Value} không tồn tại trong hệ thống." });
        }

        // 4. Tạo bản ghi ProductionTracking mới
        var tracking = new ProductionTracking
        {
            SerialNumberId = serial.SerialNumberId,
            WorkOrderId = serial.WorkOrderId,
            MachineId = dto.MachineId,
            OperatorId = dto.OperatorId,
            ProcessName = dto.ProcessName,
            Result = dto.Result,
            DefectId = dto.Result == "Fail" ? dto.DefectId : null, // Chỉ lưu DefectId nếu quét lỗi (Fail)
            ScanSource = dto.ScanSource,
            LoggedAt = DateTime.Now
        };

        _context.ProductionTrackings.Add(tracking);

        // 5. Cập nhật trạng thái SerialNumber
        if (dto.Result == "Fail")
        {
            serial.Status = SerialStatus.Fail;
            _context.SerialNumbers.Update(serial);
        }
        else if (ProductionConfig.FinalProcesses.Contains(dto.ProcessName))
        {
            // Kiểm tra xem trong lịch sử quét của serial này có bản ghi nào bị Fail hay không
            var hasFailureInHistory = await _context.ProductionTrackings
                .AnyAsync(pt => pt.SerialNumberId == serial.SerialNumberId && pt.Result == "Fail");

            serial.Status = hasFailureInHistory ? SerialStatus.Fail : SerialStatus.Pass;
            _context.SerialNumbers.Update(serial);
        }

        await _context.SaveChangesAsync();

        // 6. Tự động hoàn thành WorkOrder nếu tất cả các Serial của nó đã hoàn thành (Pass, Fail, Cancelled)
        var workOrder = await _context.WorkOrders.FindAsync(serial.WorkOrderId);
        if (workOrder != null && workOrder.Status != WorkOrderStatus.Completed && workOrder.Status != WorkOrderStatus.Cancelled)
        {
            var totalSerialsCount = await _context.SerialNumbers
                .CountAsync(s => s.WorkOrderId == workOrder.WorkOrderId);

            var finishedSerialsCount = await _context.SerialNumbers
                .CountAsync(s => s.WorkOrderId == workOrder.WorkOrderId && 
                                 (s.Status == SerialStatus.Pass || s.Status == SerialStatus.Fail || s.Status == SerialStatus.Cancelled));

            // Nếu số lượng đã quét quyết định (Pass/Fail) + đã hủy bằng đúng số lượng ban đầu của WorkOrder
            if (finishedSerialsCount >= workOrder.Quantity)
            {
                workOrder.Status = WorkOrderStatus.Completed;
                _context.WorkOrders.Update(workOrder);
                await _context.SaveChangesAsync();
            }
        }

        return Ok(new { message = "Ghi nhận thông tin quét thành công.", trackingId = tracking.ProductionTrackingId });
    }

    // GET: api/ProductionTracking/history/{serialCode}
    // ✅ Lấy lịch sử hành trình sản xuất của một mã sản phẩm
    [HttpGet("history/{serialCode}")]
    public async Task<ActionResult<IEnumerable<ProductionTrackingHistoryDto>>> GetHistory(string serialCode)
    {
        var history = await _context.ProductionTrackings
            .Include(pt => pt.SerialNumber)
            .Include(pt => pt.Machine)
            .Include(pt => pt.Operator)
            .Include(pt => pt.Defect)
            .Where(pt => pt.SerialNumber != null && pt.SerialNumber.SerialCode == serialCode)
            .OrderBy(pt => pt.LoggedAt)
            .Select(pt => new ProductionTrackingHistoryDto
            {
                ProcessName = pt.ProcessName,
                Result = pt.Result,
                DefectCode = pt.Defect != null ? pt.Defect.DefectCode : null,
                DefectName = pt.Defect != null ? pt.Defect.DefectName : null,
                MachineName = pt.Machine != null ? pt.Machine.MachineName : null,
                OperatorName = pt.Operator != null ? pt.Operator.UserName : null,
                ScanSource = pt.ScanSource,
                LoggedAt = pt.LoggedAt
            })
            .ToListAsync();

        return Ok(history);
    }
}
