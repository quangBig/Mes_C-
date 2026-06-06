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
public class WorkOrderController : ControllerBase
{
    private readonly MESDbContext _context;

    public WorkOrderController(MESDbContext context) => _context = context;

    // GET: api/WorkOrder/statuses
    [HttpGet("statuses")]
    public ActionResult<IEnumerable<WorkOrderStatusOption>> GetStatuses()
        => Ok(WorkOrderStatusList.AllOptions);

    // GET: api/WorkOrder
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderResponseDto>>> GetAll()
    {
        var list = await _context.WorkOrders
            .Include(w => w.Product)
            .Include(w => w.SerialNumbers)
            .Include(w => w.CreatedByUser)
            .Select(w => new WorkOrderResponseDto
            {
                WorkOrderId = w.WorkOrderId,
                WorkOrderCode = w.WorkOrderCode,
                ProductId = w.ProductId,
                ProductCode = w.Product != null ? w.Product.ProductCode : string.Empty,
                ProductName = w.Product != null ? w.Product.ProductName : string.Empty,
                Quantity = w.Quantity,
                CreatedBy = w.CreatedBy,
                CreatedByUserName = w.CreatedByUser != null ? w.CreatedByUser.UserName : string.Empty,
                StartTime = w.StartTime,
                EndTime = w.EndTime,
                CreatedAt = w.CreatedAt,
                Status = w.Status,
                SerialNumbers = w.SerialNumbers.Select(s => new SerialNumberDto
                {
                    SerialNumberId = s.SerialNumberId,
                    SerialCode = s.SerialCode,
                    Status = s.Status
                }).ToList()
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET: api/WorkOrder/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrderResponseDto>> Get(int id)
    {
        var w = await _context.WorkOrders
            .Include(w => w.Product)
            .Include(w => w.SerialNumbers)
            .Include(w => w.CreatedByUser)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (w == null) return NotFound();

        return Ok(new WorkOrderResponseDto
        {
            WorkOrderId = w.WorkOrderId,
            WorkOrderCode = w.WorkOrderCode,
            ProductId = w.ProductId,
            ProductCode = w.Product?.ProductCode ?? string.Empty,
            ProductName = w.Product?.ProductName ?? string.Empty,
            Quantity = w.Quantity,
            CreatedBy = w.CreatedBy,
            CreatedByUserName = w.CreatedByUser?.UserName ?? string.Empty,
            StartTime = w.StartTime,
            EndTime = w.EndTime,
            CreatedAt = w.CreatedAt,
            Status = w.Status,
            SerialNumbers = w.SerialNumbers.Select(s => new SerialNumberDto
            {
                SerialNumberId = s.SerialNumberId,
                SerialCode = s.SerialCode,
                Status = s.Status
            }).ToList()
        });
    }

    // POST: api/WorkOrder
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<WorkOrderResponseDto>> Create([FromBody] CreateWorkOrderDto dto)
    {
        // Validation: User created phải tồn tại
        var user = await _context.Users.FindAsync(dto.CreatedBy);
        if (user == null)
        {
            return BadRequest("User created không tồn tại.");
        }

        // Validation: Product phải tồn tại
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return BadRequest("Sản phẩm không tồn tại.");

        // Validation: Quantity > 0 (đã có [Range] trong DTO, thêm tường minh)
        if (dto.Quantity <= 0)
            return BadRequest("Số lượng phải lớn hơn 0.");

        // Validation: EndTime > StartTime
        if (dto.EndTime <= dto.StartTime)
            return BadRequest("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

        // Validation: WorkOrderCode không được trùng
        var codeExists = await _context.WorkOrders
            .AnyAsync(w => w.WorkOrderCode == dto.WorkOrderCode);
        if (codeExists)
            return BadRequest($"WorkOrderCode '{dto.WorkOrderCode}' đã tồn tại.");

        // Tạo WorkOrder
        var workOrder = new WorkOrder
        {
            WorkOrderCode = dto.WorkOrderCode,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            CreatedBy = dto.CreatedBy,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            CreatedAt = DateTime.Now,
            Status = WorkOrderStatus.Pending
        };

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(); // Lưu để có WorkOrderId

        // Tự động sinh Serial Numbers
        var serials = Enumerable.Range(1, dto.Quantity)
            .Select(i => new SerialNumber
            {
                WorkOrderId = workOrder.WorkOrderId,
                SerialCode = $"{product.ProductCode}-{dto.WorkOrderCode}-{i:D4}",
                Status = SerialStatus.Pending
            })
            .ToList();

        _context.SerialNumbers.AddRange(serials);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = workOrder.WorkOrderId }, new WorkOrderResponseDto
        {
            WorkOrderId = workOrder.WorkOrderId,
            WorkOrderCode = workOrder.WorkOrderCode,
            ProductId = workOrder.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            Quantity = workOrder.Quantity,
            CreatedBy = workOrder.CreatedBy,
            CreatedByUserName = user.UserName,
            StartTime = workOrder.StartTime,
            EndTime = workOrder.EndTime,
            CreatedAt = workOrder.CreatedAt,
            Status = workOrder.Status,
            SerialNumbers = serials.Select(s => new SerialNumberDto
            {
                SerialNumberId = s.SerialNumberId,
                SerialCode = s.SerialCode,
                Status = s.Status
            }).ToList()
        });
    }

    // PUT: api/WorkOrder/{id}
    //  Không thể sửa Quantity và ProductId sau khi đã sinh Serial Numbers.
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkOrderDto dto)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);
        if (workOrder == null) return NotFound();

        // Validation: User created phải tồn tại
        var userExists = await _context.Users.AnyAsync(u => u.Id == dto.CreatedBy);
        if (!userExists)
        {
            return BadRequest("User created không tồn tại.");
        }

        // Validation: EndTime > StartTime
        if (dto.EndTime <= dto.StartTime)
            return BadRequest("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

        // Validation: WorkOrderCode không được trùng với WorkOrder khác
        var codeExists = await _context.WorkOrders
            .AnyAsync(w => w.WorkOrderCode == dto.WorkOrderCode && w.WorkOrderId != id);
        if (codeExists)
            return BadRequest($"WorkOrderCode '{dto.WorkOrderCode}' đã tồn tại.");

        //  Chỉ cho phép sửa 4 trường — Quantity & ProductId bị khóa
        workOrder.WorkOrderCode = dto.WorkOrderCode;
        workOrder.CreatedBy = dto.CreatedBy;
        workOrder.StartTime = dto.StartTime;
        workOrder.EndTime = dto.EndTime;
        workOrder.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PATCH: api/WorkOrder/{id}/cancel
    // Hủy WorkOrder và đánh dấu các Serial Pending → Cancelled
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.SerialNumbers)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (workOrder == null) return NotFound();

        if (workOrder.Status == WorkOrderStatus.Completed)
            return BadRequest("Không thể hủy WorkOrder đã hoàn thành.");

        // Hủy WorkOrder (nếu chưa Cancelled)
        workOrder.Status = WorkOrderStatus.Cancelled;

        // Đánh dấu tất cả Serial còn Pending → Cancelled
        var pendingSerials = workOrder.SerialNumbers
            .Where(s => s.Status == SerialStatus.Pending)
            .ToList();

        foreach (var serial in pendingSerials)
        {
            serial.Status = SerialStatus.Cancelled;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "WorkOrder đã được hủy.",
            cancelled = pendingSerials.Count,
            workOrderId = workOrder.WorkOrderId
        });
    }

    // GET: api/WorkOrder/{id}/stats
    // ✅ Thống kê chi tiết tiến độ sản xuất và hiệu suất Yield của Lệnh sản xuất
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetStats(int id)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);
        if (workOrder == null) return NotFound();

        int quantity = workOrder.Quantity;

        int pass = await _context.SerialNumbers
            .CountAsync(s => s.WorkOrderId == id && s.Status == SerialStatus.Pass);

        int fail = await _context.SerialNumbers
            .CountAsync(s => s.WorkOrderId == id && s.Status == SerialStatus.Fail);

        int processed = pass + fail;

        double yield = 0;
        if (processed > 0)
        {
            yield = Math.Round(((double)pass / processed) * 100, 2);
        }

        double defectRate = 0;
        if (processed > 0)
        {
            defectRate = Math.Round(((double)fail / processed) * 100, 2);
        }

        return Ok(new
        {
            quantity = quantity,
            processed = processed,
            pass = pass,
            fail = fail,
            yield = yield,
            defectRate = defectRate
        });
    }
}
