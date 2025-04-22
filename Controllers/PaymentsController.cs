using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using APIBase.Models.POS;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using APIBase.Models.ReportsModels;
using Microsoft.CodeAnalysis.Operations;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class PaymentsController : ControllerBase
{
    private readonly POSContext _context;

    public PaymentsController(POSContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayment()
    {
        var excludedIds = new List<string>() { "pt-loy" };
        return await _context.Payments.Include(x => x.PaymentType).Include(x => x.Status).Where(x => !excludedIds.Contains(x.Id)).ToListAsync();
    }

    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayment()
    {
        return await _context.Payments.Include(x => x.PaymentType).Include(x => x.Status).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> GetPayment(string id)
    {
        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
        {
            return NotFound();
        }

        return payment;
    }


    [HttpGet("GetByWorkDayId/{id}")]
    public async Task<IActionResult> GetPaymentsByWorkDayId(Guid id)
    {
        //var m = await _context.OrderPayments.CountAsync();

        var orderPaymentsGroup = await _context.OrderPayments
            .GroupBy(op => new { op.OrderHeader.BranchId, op.PaymentId })
            .Select(g => new
            {
                BranchId = g.Key.BranchId,
                PaymentId = g.Key.PaymentId,
                Total = g.Sum(g => g.Amount)
            })
            .OrderBy(x => x.BranchId)
            .ThenBy(x => x.PaymentId)
            .ToListAsync();

        List<Payment> payments = await _context.Payments
         .AsNoTracking()
         .ToListAsync();

        //IEnumerable<WorkDayDetailsModel> result = orderPaymentsGroup
        //   .GroupJoin(
        //       payments,
        //       group => group.PaymentId,
        //       payment => payment.Id,
        //       (group, payment) => new WorkDayDetailsModel()
        //       {
        //           PaymentId = group.PaymentId,
        //           PaymentName = payment.FirstOrDefault().Name,
        //           PaymentSname = payment.FirstOrDefault().Sname,
        //           BranchId = group.BranchId,
        //           Total = group.Total
        //       }
        //   )
        //   .ToList();

        //var paymentIdSumGroup = await _context.OrderPayments
        //    .Where(op => op.OrderHeader.WorkDayId == id)
        //    .GroupBy(op => op.PaymentId)
        //    .Select(op => new
        //    {
        //        Id = op.Key,
        //        Total = op.Sum(g => g.Amount)
        //    })
        //    .ToListAsync();

        //WorkDayDetailsModel? x = await _context.WorkDays
        //    .Where(wd => wd.Id == id)
        //    .Include(wd => wd.OrderHeaders)
        //    .ThenInclude(oh => oh.OrderPayments)
        //    .Select(wd => new WorkDayDetailsModel()
        //    {
        //        Id = wd.Id,
        //        BranchId = wd.BranchId,
        //        OrderPayments = wd.OrderHeaders
        //            .SelectMany(oh => oh.OrderPayments)
        //            .SelectMany

        //    })
        //    .FirstOrDefaultAsync();

        return Ok();
    }


    [HttpGet("Short")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetShortPayment()
    {
        var excludedIds = new List<string>() { "pt-loy" };
        return await _context.Payments.Where(x => !excludedIds.Contains(x.Id) && x.StatusId == "st-active").ToListAsync();
    }
    private bool PaymentExists(string id)
    {
        return _context.Payments.Any(e => e.Id == id);
    }
}
