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
}
