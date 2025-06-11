using System.Collections.Frozen;
using System.Diagnostics;
using APIBase.Models.DTOs.ApiOrder;
using APIBase.Models.DTOs.OrderHeader;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Models.ReportsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
/// IMPORTANT Override Authorize//
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class EmployeesController : ControllerBase
{
    private readonly POSContext _context;
    private readonly forkpos_masterContext _userDBContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EmployeesController(POSContext context, forkpos_masterContext userDBContext, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userDBContext = userDBContext;
        _webHostEnvironment = webHostEnvironment;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployee()
    {
        var userId = User.Claims.Where(x => x.Type == "EmployeeId").FirstOrDefault().Value;
        var excludedIds = new List<string>() { "api", "kiosk" };

        if (userId != "admin")
            excludedIds.Add("admin");
        return await _context.Employees.Where(x => !excludedIds.Contains(x.Id) && x.StatusId != "st-deleted").Include(x => x.Branch).Include(x => x.EmployeeGroup).Include(x => x.PermRole).Include(x => x.Status).AsNoTracking().ToListAsync();
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(string id)
    {
        var employee = await _context.Employees.Include(x => x.Branch).Include(x => x.EmployeeGroup).Include(x => x.PermRole).ThenInclude(x => x.PermRolePerms).Include(x => x.Status).AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        return employee;
    }

    [Authorize]
    [HttpGet("{id}/VoidOrders")]
    public async Task<ActionResult<VoidOrderByEmployeeSummary>> GetVoidOrders(string id, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(emp => emp.Id == id);

        var voidedOrderHeadersIds = await _context.OrderItems
            .AsNoTracking()
            .Where
            (
                oi =>
                    oi.Void &&
                    oi.VoidBy == id &&
                    oi.VoidAt.Value.Date >= from.Date &&
                    oi.VoidAt.Value.Date <= to.Date
            )
            .Select(oi => oi.OrderHeaderId)
            .ToListAsync();

        FrozenDictionary<string, VoidType> voidTypes = _context.VoidTypes.ToFrozenDictionary(emp => emp.Id);

        List<VoidOrderSummary> voidedOrderHeaders = await _context.OrderHeaders
            .AsNoTracking()
            .Where(oh => voidedOrderHeadersIds.Contains(oh.Id))
            .Select(oh => new VoidOrderSummary()
            {
                Id = oh.Id,
                OrderNumber = oh.OrderNumber,
                IsVoid = oh.OrderItems.All(oi => oi.Void),
                VoidAt = oh.VoidAt.Value,
                VoidReasonId = oh.VoidReasonId,
                VoidTypeId = oh.VoidTypeId,
                Total = oh.Total,
                TotalVoid = oh.OrderItems.Where(oi => oi.Void).Sum(oi => oi.Total),
            })
            .ToListAsync();

        return new VoidOrderByEmployeeSummary()
        {
            Employee = employee,
            VoidOrders = voidedOrderHeaders
        };
    }
}

