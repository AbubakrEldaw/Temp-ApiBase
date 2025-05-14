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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetApiOrder(string id)
    {
        try
        {
            var apiOrder = await _context.ApiOrders
                .Where(x => x.Id == Guid.Parse(id))
                .Include(x => x.StagingStatus)
                .Include(x => x.ApiOrderStatusHistories)
                    .ThenInclude(x => x.ApiStatus)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (apiOrder == null)
                return NoContent();

            if (apiOrder?.ApiOrderStatusHistories != null)
            {
                apiOrder.ApiOrderStatusHistories = apiOrder.ApiOrderStatusHistories
                    .OrderByDescending(h => h.ApiStatusId)
                    .ThenByDescending(h => h.StatusTime)
                    .ToList();
            }

            Guid? orderHeaderId = JsonConvert.DeserializeObject<OrderHeader>(apiOrder.OrderModel)?.Id;

            if (orderHeaderId == null)
            {
                return NoContent();
            }


            OrderHeader? orderHeader = await _context.OrderHeaders
                .Where(oh => oh.Id == orderHeaderId.Value)
                .Include(oh => oh.OrderItems)
                .Include(oh => oh.DiningOption)
                .Include(oh => oh.OrderSource)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (orderHeader == null)
            {
                return NoContent();
            }

            ApiOrderSummary response = new()
            {
                ApiOrder = apiOrder,
                DiningOption = new Models.LocalizedName()
                {
                    Name = orderHeader.DiningOption.Name,
                    Sname = orderHeader.DiningOption.Sname
                },
                OrderSource = new Models.LocalizedName()
                {
                    Name = orderHeader.OrderSource.Name,
                    Sname = orderHeader.OrderSource.Sname
                },
                Items = orderHeader.OrderItems.Select(oi => new OrderItemSummary()
                {
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Total = oi.Total,
                    Name = new Models.LocalizedName()
                    {
                        Name = oi.Item?.Name ?? "",
                        Sname = oi.Item?.Sname ?? "",
                    }
                })
                .ToList(),
                Note = orderHeader.Note,
                DiscountAmount = orderHeader.HeaderDiscountAmount,
                VatAmount = orderHeader.TotalVat,
                Total = orderHeader.Total,
            };

            if (!User.IsInRole("admin"))
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

