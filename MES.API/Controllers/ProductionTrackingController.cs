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
    //  Ghi nhận quét mã Serial Number tại công đoạn sản xuất
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
        if (dto.Result == TrackingResult.Fail && dto.DefectId.HasValue)
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
            DefectId = dto.Result == TrackingResult.Fail ? dto.DefectId : null, // Chỉ lưu DefectId nếu quét lỗi (Fail)
            ScanSource = dto.ScanSource,
            LoggedAt = DateTime.Now
        };

        _context.ProductionTrackings.Add(tracking);

        // 5. Cập nhật trạng thái SerialNumber
        if (dto.Result == TrackingResult.Fail)
        {
            serial.Status = SerialStatus.Fail;
            _context.SerialNumbers.Update(serial);
        }
        else if (ProductionConfig.FinalProcesses.Contains(dto.ProcessName))
        {
            // Kiểm tra xem trong lịch sử quét của serial này có bản ghi nào bị Fail hay không
            var hasFailureInHistory = await _context.ProductionTrackings
                .AnyAsync(pt => pt.SerialNumberId == serial.SerialNumberId && pt.Result == TrackingResult.Fail);

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
    //  Lấy lịch sử hành trình sản xuất của một mã sản phẩm
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

    // POST: api/ProductionTracking/simulate
    // ✅ Giả lập quét sản xuất tự động cho toàn bộ Serial Numbers của một WorkOrder
    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulateDto dto)
    {
        // 1. Kiểm tra WorkOrder có tồn tại không
        var workOrder = await _context.WorkOrders
            .Include(w => w.SerialNumbers)
            .FirstOrDefaultAsync(w => w.WorkOrderId == dto.WorkOrderId);

        if (workOrder == null)
            return NotFound(new { message = $"Lệnh sản xuất với Id {dto.WorkOrderId} không tồn tại." });

        if (workOrder.Status == WorkOrderStatus.Completed || workOrder.Status == WorkOrderStatus.Cancelled)
            return BadRequest(new { message = "Lệnh sản xuất đã hoàn thành hoặc đã bị hủy, không thể tiến hành giả lập." });

        // 2. Kiểm tra Operator có tồn tại không
        var operatorUser = await _context.Users.FindAsync(dto.OperatorId);
        if (operatorUser == null)
            return BadRequest(new { message = $"Người vận hành với Id {dto.OperatorId} không tồn tại trong hệ thống." });

        // 3. Kiểm tra tính hợp lệ của danh sách các máy
        var machineIds = dto.Steps.Select(s => s.MachineId).Distinct().ToList();
        var machinesDb = await _context.Machines.Where(m => machineIds.Contains(m.MachineId)).ToListAsync();
        if (machinesDb.Count < machineIds.Count)
            return BadRequest(new { message = "Một hoặc nhiều MachineId không tồn tại trong hệ thống." });

        // 4. Lấy danh sách lỗi có sẵn trong DB để sinh ngẫu nhiên an toàn
        var defects = await _context.Defects.ToListAsync();

        // 5. Lọc danh sách các Serial Numbers đang ở trạng thái Pending
        var pendingSerials = workOrder.SerialNumbers
            .Where(s => s.Status == SerialStatus.Pending)
            .ToList();

        if (pendingSerials.Count == 0)
            return BadRequest(new { message = "Không có Serial Number nào ở trạng thái Pending để tiến hành giả lập." });

        // Chuyển trạng thái Lệnh sản xuất sang InProgress
        if (workOrder.Status == WorkOrderStatus.Pending)
        {
            workOrder.Status = WorkOrderStatus.InProgress;
            _context.WorkOrders.Update(workOrder);
        }

        // 6. Tính toán số lượng Serial sẽ bị lỗi dựa trên defectRate
        var random = new Random();
        int totalToFail = (int)Math.Round(pendingSerials.Count * (dto.DefectRate / 100.0));
        var failedSerials = pendingSerials.OrderBy(x => random.Next()).Take(totalToFail).ToHashSet();

        // 7. Tiến hành giả lập quét cho từng Serial
        foreach (var serial in pendingSerials)
        {
            // Bắt đầu thời điểm quét ban đầu lùi lại so với hiện tại
            var currentTime = DateTime.Now.AddMinutes(-dto.Steps.Count * dto.IntervalMinutes);

            // Quyết định công đoạn lỗi ngẫu nhiên nếu Serial này nằm trong tập lỗi
            int failStepIndex = -1;
            if (failedSerials.Contains(serial))
            {
                failStepIndex = random.Next(dto.Steps.Count);
            }

            for (int i = 0; i < dto.Steps.Count; i++)
            {
                // Giãn cách thời gian thực tế: dao động xung quanh IntervalMinutes
                currentTime = currentTime.AddMinutes(Math.Max(1, dto.IntervalMinutes + random.Next(-1, 2)));

                var step = dto.Steps[i];
                var machine = machinesDb.First(m => m.MachineId == step.MachineId);
                var processName = GetProcessNameFromMachine(machine.MachineName);
                bool isFailStep = (i == failStepIndex);

                int? defectId = null;
                if (isFailStep && defects.Count > 0)
                {
                    defectId = defects[random.Next(defects.Count)].DefectId;
                }

                var tracking = new ProductionTracking
                {
                    SerialNumberId = serial.SerialNumberId,
                    WorkOrderId = workOrder.WorkOrderId,
                    MachineId = machine.MachineId,
                    OperatorId = dto.OperatorId,
                    ProcessName = processName,
                    Result = isFailStep ? TrackingResult.Fail : TrackingResult.Pass,
                    DefectId = defectId,
                    ScanSource = ScanSourceType.Simulation,
                    LoggedAt = currentTime
                };
                _context.ProductionTrackings.Add(tracking);

                if (isFailStep)
                {
                    serial.Status = SerialStatus.Fail;
                }

                // Nếu là công đoạn cuối cùng trong danh sách truyền lên
                if (i == dto.Steps.Count - 1)
                {
                    if (serial.Status != SerialStatus.Fail)
                    {
                        serial.Status = SerialStatus.Pass;
                    }
                }
            }
            _context.SerialNumbers.Update(serial);
        }

        await _context.SaveChangesAsync();

        // 8. Tự động hoàn thành WorkOrder sau khi tất cả Serial hoàn thành
        int totalSerials = workOrder.SerialNumbers.Count;
        int passCount = workOrder.SerialNumbers.Count(s => s.Status == SerialStatus.Pass);
        int failCount = workOrder.SerialNumbers.Count(s => s.Status == SerialStatus.Fail);
        double yield = totalSerials > 0 ? Math.Round(((double)passCount / totalSerials) * 100, 2) : 0;

        workOrder.Status = WorkOrderStatus.Completed;
        _context.WorkOrders.Update(workOrder);
        await _context.SaveChangesAsync();

        var response = new SimulateResponseDto
        {
            WorkOrderId = workOrder.WorkOrderId,
            TotalSerials = totalSerials,
            PassCount = passCount,
            FailCount = failCount,
            Yield = yield
        };

        return Ok(response);
    }

    private string GetProcessNameFromMachine(string machineName)
    {
        var name = machineName.ToLower();
        if (name.Contains("printer") || name.Contains("in kem")) return "Printer";
        if (name.Contains("spi")) return "SPI";
        if (name.Contains("mounter") || name.Contains("gắp")) return "Mounter";
        if (name.Contains("reflow") || name.Contains("lò hàn")) return "Reflow";
        if (name.Contains("aoi")) return "AOI";
        if (name.Contains("packing") || name.Contains("đóng gói")) return "Packing";
        if (name.Contains("fct")) return "FCT";
        if (name.Contains("ict")) return "ICT";
        return machineName;
    }
}
