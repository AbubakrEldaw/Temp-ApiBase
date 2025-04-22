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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;

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
        //var orderPaymentsGroup = await _context.OrderPayments
        //    .GroupBy(op => new { op.OrderHeader.BranchId, op.PaymentId })
        //    .Select(g => new
        //    {
        //        BranchId = g.Key.BranchId,
        //        PaymentId = g.Key.PaymentId,
        //        Total = g.Sum(g => g.Amount)
        //    })
        //    .OrderBy(x => x.BranchId)
        //    .ThenBy(x => x.PaymentId)
        //    .ToListAsync();

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

        var x = await _context.WorkDays.Where(x => x.Id == id).FirstOrDefaultAsync();

        var paymentIdSumGroup = await _context.OrderPayments
            .Where(op => op.OrderHeader.WorkDayId == id)
            .GroupBy(op => new { op.PaymentId, op.OrderHeader.IsReturn })
            .Select(opGroup => new
            {
                PaymentId = opGroup.Key.PaymentId,
                Total = opGroup.Sum(r => opGroup.Key.IsReturn ? (r.Amount * -1) : r.Amount)

                //Blah = new {
                //    SalesOrdersCount = opGroup.FirstOrDefault().OrderHeader.WorkDay.OrderHeaders.Sum(x => !x.IsReturn ? 1 : 0),
                //    SalesOrdersCountReturn = opGroup.FirstOrDefault().OrderHeader.WorkDay.OrderHeaders.Sum(x => x.IsReturn ? 1 : 0),
                //    CustomerCount = opGroup.FirstOrDefault().OrderHeader.WorkDay.OrderHeaders.Sum(x => x.CustomerId != null ? 1 : 0),
                //    GuestCount = opGroup.FirstOrDefault().OrderHeader.WorkDay.OrderHeaders.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0)
                //},
                //WorkDaySummary = new SalesReportByWorkDayModel()
                //{
                //    BranchId = opGroup.FirstOrDefault().OrderHeader.WorkDay.Branch.Id,
                //    Date = opGroup.FirstOrDefault().OrderHeader.WorkDay.Date.ToString("yyyy-MM-dd"),
                //    OpenAt = opGroup.FirstOrDefault().OrderHeader.WorkDay.OpenAt.ToString("yyyy-MM-dd"),
                //    OpenBy = opGroup.FirstOrDefault().OrderHeader.WorkDay.OpenByNavigation.Name,
                //    CloseAt = opGroup.FirstOrDefault().OrderHeader.WorkDay.CloseAt.Value.ToString("yyyy-MM-dd"),
                //    CloseBy = opGroup.FirstOrDefault().OrderHeader.WorkDay.CloseByNavigation.Name,

                //NetSales = opGroup.FirstOrDefault().OrderHeader.WorkDay.,
                //VatAmount = opGroup.FirstOrDefault().OrderHeader.WorkDay.VatAmount,
                //NetSalesWithTax = opGroup.FirstOrDefault().OrderHeader.WorkDay.NetSalesWithTax,
                //DiscountAmount = opGroup.FirstOrDefault().OrderHeader.WorkDay.DiscountAmount,
                //GrossSales = opGroup.FirstOrDefault().OrderHeader.WorkDay.GrossSales,

                //NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
                //VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                //NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                //DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
                //GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
                //},
                //BranchName = opGroup.FirstOrDefault().OrderHeader.WorkDay.Branch.Name,
            })
            .OrderBy(g => g.PaymentId)
            .ToListAsync();

        paymentIdSumGroup = paymentIdSumGroup
            .GroupBy(opGroup => opGroup.PaymentId)
            .Select(opGroup2 => new
            {
                PaymentId = opGroup2.Key,
                //WorkDaySummary = opGroup2.FirstOrDefault().WorkDaySummary,
                //BranchName = opGroup2.FirstOrDefault().BranchName,
                Total = opGroup2.Sum(r => r.Total)
            })
            .ToList();

        List<Payment> payments = await _context.Payments
            .AsNoTracking()
            .ToListAsync();

        IEnumerable<WorkDayDetailsModel> result = paymentIdSumGroup
           .GroupJoin(
               payments,
               group => group.PaymentId,
               payment => payment.Id,
               (group, payment) => new WorkDayDetailsModel()
               {
                   PaymentId = group.PaymentId,
                   PaymentName = payment.FirstOrDefault().Name,
                   PaymentSname = payment.FirstOrDefault().Sname,
                   //WorkDaySummary = group.WorkDaySummary,
                   Total = group.Total,
               }
           )
           .ToList();

        //WorkDayDetailsModel x = await _context.WorkDays
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

        return Ok(result);
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
