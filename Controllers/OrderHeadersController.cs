using APIBase.Helpers;
using APIBase.Models.DTOs.ApiOrder;
using APIBase.Models.DTOs.OrderHeader;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class OrderHeadersController : ControllerBase
{
    private readonly POSContext _context;
    private readonly forkpos_masterContext _masterContext;
    private readonly IEncMaster _encMaster;


    public OrderHeadersController(POSContext context, forkpos_masterContext masterContext, IEncMaster encMaster)
    {
        _context = context;
        _masterContext = masterContext;
        _encMaster = encMaster;
    }
 
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderHeader>> GetOrderHeader(string id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        try
        {
            var orderHeader = await _context.OrderHeaders.Where(x => x.Id.ToString() == id)
                                                     .Include(x => x.OrderItems.Where(x => !x.Modifier && !x.Void)).ThenInclude(x => x.Item)
                                                     .Include(x => x.OrderItems.Where(x => !x.Modifier && !x.Void)).ThenInclude(x => x.InverseModifierParentNavigation).ThenInclude(x => x.Item)
                                                     .Include(x => x.OrderItems.Where(x => !x.Modifier && !x.Void)).ThenInclude(x => x.OrderItemCustomerComments)
                                                     .Include(x => x.OrderPayments).ThenInclude(x => x.Payment)
                                                     .Include(x => x.OrderFees)
                                                     .Include(x => x.OrderSource)
                                                     .Include(x => x.DiningOption)
                                                     .Include(x => x.Branch).ThenInclude(x => x.ReceiptSetting)
                                                     .Include(x => x.Waiter)
                                                     .AsSplitQuery().AsNoTracking().FirstOrDefaultAsync();

            if (orderHeader == null)
                return NoContent();

            // Commit the transaction
            await transaction.CommitAsync();

            return Ok(orderHeader);
        }
        catch (Exception ex)
        {
            // Handle exceptions and potentially roll back the transaction
            await transaction.RollbackAsync();
            // Log the exception or handle it as per your error handling policy
            return StatusCode(500, "Internal Server Error: " + ex.Message);
        }
    }

    [Authorize]
    [HttpGet("{id}/VoidDetails")]
    public async Task<ActionResult<VoidOrderSummary>> GetVoidOrderDetails(Guid id)
    {
        var oi = await _context.OrderItems
            .Where(oi => oi.OrderHeaderId == id)
            .Select(oi => new OrderItemSummary()
            {

                Quantity = oi.Quantity,
                Price = oi.Price,
                Total = oi.Total,
                IsVoid = oi.Void,
                VoidTypeId = oi.VoidTypeId,
                Name = new Models.LocalizedName()
                {
                    Name = oi.Item.Name ?? "",
                    Sname = oi.Item.Sname ?? "",
                }
            })
            .OrderByDescending(oi => oi.Total)
            .ToListAsync();

        var oh = await _context.OrderHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        var response = new VoidOrderSummary()
        {
            Id = oh.Id,
            OrderNumber = oh.OrderNumber,
            IsVoid = oh.OrderItems.All(oi => oi.Void),
            VoidAt = oh.VoidAt,
            VoidTypeId = oh.VoidTypeId,
            VoidReasonId = oh.VoidReasonId,
            Total = oh.Total,
            TotalVoid = oh.OrderItems.Where(oi => oi.Void).Sum(oi => oi.Total),
            Items = oi
        };

        return response;
    }
}

