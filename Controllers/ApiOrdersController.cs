using APIBase.Helpers;
using APIBase.Models.DTOs.ApiOrder;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Xml.Linq;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class ApiOrdersController : ControllerBase
{
    private readonly POSContext _context;
    private readonly forkpos_masterContext _masterContext;
    private readonly IEncMaster _encMaster;


    public ApiOrdersController(POSContext context, forkpos_masterContext masterContext, IEncMaster encMaster)
    {
        _context = context;
        _masterContext = masterContext;
        _encMaster = encMaster;
    }

    // todo: handle admin and stuff
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderHeader>> GetApiOrder(string id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        try
        {
            var apiOrder = await _context.ApiOrders.Where(x => x.Id == Guid.Parse(id))
                .Include(x => x.StagingStatus)
                .Include(x => x.ApiOrderStatusHistories)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (apiOrder == null)
                return NoContent();

            OrderHeader? orderHeader = JsonConvert.DeserializeObject<OrderHeader>(apiOrder.OrderModel);

            if (orderHeader == null)
            {
                return NoContent();
            }

            ApiOrderSummary response = new () {
                ApiOrder = apiOrder,
                Items = orderHeader.OrderItems.Select(oi => new OrderItemSummary ()
                {
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Total = oi.Total,
                    DiscountAmount = oi.DiscountAmount,
                    Name = oi.Item.Name,
                    Sname = oi.Item.Sname,
                })
                .ToList(),
            };

            // admin perms
            if (!true)
            {
                response.ApiOrder.OrderModel = null;
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error: " + ex.Message);
        }
    }

}

