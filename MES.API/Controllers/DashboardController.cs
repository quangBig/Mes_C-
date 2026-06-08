using System;
using System.Linq;
using System.Threading.Tasks;
using MES.API.Data;
using MES.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MES.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly MESDbContext _context;

    public DashboardController(MESDbContext context)
    {
        _context = context;
    }

    // GET: api/Dashboard/summary
    // ✅ Lấy tổng quát các thông số vận hành của toàn nhà máy
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        // 1. Tổng số lượng Lệnh sản xuất (WorkOrders)
        var totalWorkOrders = await _context.WorkOrders.CountAsync();

        // 2. Số lượng WorkOrders đang chạy
        var runningWorkOrders = await _context.WorkOrders
            .CountAsync(w => w.Status == WorkOrderStatus.InProgress);

        // 3. Số lượng WorkOrders đã hoàn thành
        var completedWorkOrders = await _context.WorkOrders
            .CountAsync(w => w.Status == WorkOrderStatus.Completed);

        // 4. Tổng số lượng Serial đạt (Pass)
        var totalPass = await _context.SerialNumbers
            .CountAsync(s => s.Status == SerialStatus.Pass);

        // 5. Tổng số lượng Serial lỗi (Fail)
        var totalFail = await _context.SerialNumbers
            .CountAsync(s => s.Status == SerialStatus.Fail);

        // 6. Tính toán Yield (hiệu suất tổng quát toàn nhà máy)
        double yield = 0;
        int totalProcessed = totalPass + totalFail;
        if (totalProcessed > 0)
        {
            yield = Math.Round(((double)totalPass / totalProcessed) * 100, 2);
        }

        return Ok(new
        {
            workOrders = totalWorkOrders,
            runningWorkOrders = runningWorkOrders,
            completedWorkOrders = completedWorkOrders,
            pass = totalPass,
            fail = totalFail,
            yield = yield
        });
    }

    // GET: api/Dashboard/workorder/{id}
    // ✅ Lấy thống kê hiệu suất chi tiết của một Lệnh sản xuất (WorkOrder)
    [HttpGet("workorder/{id}")]
    public async Task<IActionResult> GetWorkOrderDashboard(int id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.SerialNumbers)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (workOrder == null)
            return NotFound(new { message = $"Lệnh sản xuất với Id {id} không tồn tại." });

        int plannedQuantity = workOrder.Quantity;
        int pass = workOrder.SerialNumbers.Count(s => s.Status == SerialStatus.Pass);
        int fail = workOrder.SerialNumbers.Count(s => s.Status == SerialStatus.Fail);
        int pending = workOrder.SerialNumbers.Count(s => s.Status == SerialStatus.Pending);
        int rework = workOrder.SerialNumbers.Count(s => s.Status == SerialStatus.Rework);
        int cancelled = workOrder.SerialNumbers.Count(s => s.Status == SerialStatus.Cancelled);
        int completedQuantity = pass + fail;

        double yield = 0;
        if (completedQuantity > 0)
        {
            yield = Math.Round(((double)pass / completedQuantity) * 100, 2);
        }

        return Ok(new
        {
            workOrderCode = workOrder.WorkOrderCode,
            status = workOrder.Status,
            plannedQuantity = plannedQuantity,
            completedQuantity = completedQuantity,
            pending = pending,
            rework = rework,
            cancelled = cancelled,
            pass = pass,
            fail = fail,
            yield = yield
        });
    }

    // GET: api/Dashboard/machines
    // ✅ Thống kê kết quả Pass/Fail và tỷ lệ Yield theo từng máy móc
    [HttpGet("machines")]
    public async Task<IActionResult> GetMachineDashboard()
    {
        var rawStats = await _context.ProductionTrackings
            .Include(pt => pt.Machine)
            .Where(pt => pt.Machine != null)
            .GroupBy(pt => new { pt.MachineId, pt.Machine!.MachineName })
            .Select(g => new
            {
                MachineId = g.Key.MachineId,
                MachineName = g.Key.MachineName,
                Pass = g.Count(pt => pt.Result == TrackingResult.Pass),
                Fail = g.Count(pt => pt.Result == TrackingResult.Fail)
            })
            .ToListAsync();

        var stats = rawStats.Select(s => new
        {
            machineId = s.MachineId,
            machineName = s.MachineName,
            pass = s.Pass,
            fail = s.Fail,
            yield = (s.Pass + s.Fail) > 0 ? Math.Round(((double)s.Pass / (s.Pass + s.Fail)) * 100, 2) : 0
        }).ToList();

        return Ok(stats);
    }

    // GET: api/Dashboard/top-defects
    // ✅ Lấy Top 5 lỗi phổ biến nhất trong quá trình sản xuất
    [HttpGet("top-defects")]
    public async Task<IActionResult> GetTopDefects()
    {
        var topDefects = await _context.ProductionTrackings
            .Include(pt => pt.Defect)
            .Where(pt => pt.Result == TrackingResult.Fail && pt.Defect != null)
            .GroupBy(pt => new { pt.DefectId, pt.Defect!.DefectName })
            .Select(g => new
            {
                defectId = g.Key.DefectId,
                defectName = g.Key.DefectName,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        return Ok(topDefects);
    }

    // GET: api/Dashboard/yield-by-day
    // ✅ Thống kê tỷ lệ Yield của công đoạn cuối theo từng ngày
    [HttpGet("yield-by-day")]
    public async Task<IActionResult> GetYieldByDay()
    {
        var finalProcesses = ProductionConfig.FinalProcesses.ToList();
        var dailyStats = await _context.ProductionTrackings
            .Where(pt => finalProcesses.Contains(pt.ProcessName))
            .GroupBy(pt => pt.LoggedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Pass = g.Count(pt => pt.Result == TrackingResult.Pass),
                Fail = g.Count(pt => pt.Result == TrackingResult.Fail)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var result = dailyStats.Select(s => new
        {
            date = s.Date.ToString("yyyy-MM-dd"),
            yield = (s.Pass + s.Fail) > 0 ? Math.Round(((double)s.Pass / (s.Pass + s.Fail)) * 100, 2) : 0
        }).ToList();

        return Ok(result);
    }

    // GET: api/Dashboard/live-production
    //  Số liệu sản xuất thời gian thực trong ngày hôm nay
    [HttpGet("live-production")]
    public async Task<IActionResult> GetLiveProduction()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var finalProcesses = ProductionConfig.FinalProcesses.ToList();
        var todayStats = await _context.ProductionTrackings
            .Where(pt => finalProcesses.Contains(pt.ProcessName) && pt.LoggedAt >= today && pt.LoggedAt < tomorrow)
            .ToListAsync();

        int todayPass = todayStats.Count(pt => pt.Result == TrackingResult.Pass);
        int todayFail = todayStats.Count(pt => pt.Result == TrackingResult.Fail);
        int totalToday = todayPass + todayFail;
        double currentYield = totalToday > 0 ? Math.Round(((double)todayPass / totalToday) * 100, 2) : 0;

        return Ok(new
        {
            todayPass = todayPass,
            todayFail = todayFail,
            currentYield = currentYield
        });
    }

    // GET: api/Dashboard/scan-sources
    // ✅ Thống kê số lượng lượt quét theo nguồn dữ liệu (ScanSource)
    [HttpGet("scan-sources")]
    public async Task<IActionResult> GetScanSourcesDashboard()
    {
        var stats = await _context.ProductionTrackings
            .GroupBy(pt => pt.ScanSource)
            .Select(g => new
            {
                Source = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var manualCount = stats.FirstOrDefault(s => s.Source == ScanSourceType.Manual)?.Count ?? 0;
        var machineCount = stats.FirstOrDefault(s => s.Source == ScanSourceType.Machine)?.Count ?? 0;
        var simulationCount = stats.FirstOrDefault(s => s.Source == ScanSourceType.Simulation)?.Count ?? 0;

        return Ok(new
        {
            manual = manualCount,
            machine = machineCount,
            simulation = simulationCount
        });
    }
}

