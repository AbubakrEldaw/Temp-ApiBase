using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using APIBase.Helpers;
using APIBase.Helpers;
using APIBase.Models.CustomModels;
using APIBase.Models.Enums;
using APIBase.Models.Enums;
using APIBase.Models.Master;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Models.POS;
using APIBase.Models.ReportsModels;
using APIBase.Models.ReportsModels;
using APIBase.Services;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Company = APIBase.Models.Master.Company;

namespace APIBase.Controllers.Reports;
public partial class ReportsController : Controller
{
    [HttpGet("Sales")]
    public async Task<ActionResult<ReportWrapper>> GetSalesReport
    (
        [FromQuery] SalesReportsGrouping reportType,
        [FromQuery] string branches,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string itemGroups = "",
        [FromQuery] string orderSources = ""
    )
    {
        try
        {
            if (!ReportModelTypes.TryGetValue(reportType, out var modelType))
                return new ObjectResult(new ApiError("400", $"Bad Request"))
                {
                    StatusCode = 400
                };

            ReportsRequest reportsRequest = new(branches, from, to, itemGroups, orderSources);

            dynamic? data = reportType switch
            {
                SalesReportsGrouping.Date => await SalesByDateAsync(reportsRequest),
                SalesReportsGrouping.Branch => await SalesByBranchAsync(reportsRequest),
                SalesReportsGrouping.Hour => await SalesByHourAsync(reportsRequest),
                SalesReportsGrouping.Item => await SalesByItemAsync(reportsRequest),
                SalesReportsGrouping.Modifier => await SalesByModifierAsync(reportsRequest),
                SalesReportsGrouping.ItemGroup => await SalesByItemGroupAsync(reportsRequest),
                SalesReportsGrouping.Employee => await SalesByEmployeeAsync(reportsRequest),
                SalesReportsGrouping.WorkDay => await SalesByWorkDayAsync(reportsRequest),
                SalesReportsGrouping.Shift => await SalesByShiftAsync(reportsRequest),
                SalesReportsGrouping.PaymentType => await SalesByPaymentTypeAsync(reportsRequest),
                SalesReportsGrouping.PaymentTypeByBranch => await SalesByPaymentTypeByBranchAsync(reportsRequest),
                SalesReportsGrouping.PaymentTypeByDate => await SalesByPaymentTypeByDateAsync(reportsRequest),
                SalesReportsGrouping.PaymentTypeByDateByBranch => await SalesByPaymentTypeByBranchAndDateAsync(reportsRequest),
                SalesReportsGrouping.OrderSource => await SalesByOrderSourceAsync(reportsRequest),
                SalesReportsGrouping.DiningOption => await SalesByDiningOptionAsync(reportsRequest),
                _ => null
            };

            if (data == null)
                return new ObjectResult(new ApiError("400", $"Bad Request"))
                {
                    StatusCode = 400
                };

            return Ok(new ReportWrapper
            {
                Report = reportType.ToString(),
                ColumnsTypes = GetModelPropertyTypes(modelType),
                Data = data
            });
        }
        catch (Exception ex)
        {
            return new ObjectResult(new ApiError("500", $"Internal server error: {ex.Message}"))
            {
                StatusCode = 500
            };
        }
    }

    private Dictionary<string, string> GetModelPropertyTypes(Type modelType)
    {
        return modelType.GetProperties()
            .ToDictionary(
                p => char.ToLowerInvariant(p.Name[0]) + p.Name.Substring(1),
                p =>
                {
                    var typeAttr = p.GetCustomAttribute<ReportFieldTypeAttribute>();
                    if (typeAttr != null)
                    {
                        return typeAttr.Type;
                    }

                    Type propType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                    return propType switch
                    {
                        _ when propType == typeof(string) => "string",
                        _ when propType == typeof(int) || propType == typeof(long) => "num",
                        _ when propType == typeof(decimal) ||
                              propType == typeof(double) ||
                              propType == typeof(float) => "num",
                        _ when propType == typeof(DateTime) ||
                              propType == typeof(DateOnly) => "date",
                        _ when propType == typeof(bool) => "boolean",
                        _ => propType.Name.ToLower()
                    };
                }
            );
    }

    private static readonly Dictionary<SalesReportsGrouping, Type> ReportModelTypes = new()
    {
        [SalesReportsGrouping.Date] = typeof(SalesReportByDateModel),
        [SalesReportsGrouping.Branch] = typeof(SalesReportByXModel),
        [SalesReportsGrouping.Hour] = typeof(SalesReportByXModel),
        [SalesReportsGrouping.Item] = typeof(SalesReportByXModel),
        [SalesReportsGrouping.Modifier] = typeof(SalesReportByXModel),
        [SalesReportsGrouping.ItemGroup] = typeof(SalesReportByXModel),
        [SalesReportsGrouping.Employee] = typeof(SalesReportByXModel),
        [SalesReportsGrouping.PaymentType] = typeof(PaymentTypeModel),
        [SalesReportsGrouping.PaymentTypeByBranch] = typeof(PaymentTypeByBranchModel),
        [SalesReportsGrouping.PaymentTypeByDate] = typeof(PaymentTypeByDateModel),
        [SalesReportsGrouping.PaymentTypeByDateByBranch] = typeof(PaymentTypeByBranchAndDateModel),
        [SalesReportsGrouping.WorkDay] = typeof(SalesReportByWorkDayModel),
        [SalesReportsGrouping.Shift] = typeof(SalesReportByShiftModel),
        [SalesReportsGrouping.OrderSource] = typeof(SalesReportByXModel),
        [SalesReportsGrouping.DiningOption] = typeof(SalesReportByXModel),
    };

    private async Task<IEnumerable<SalesReportByDateModel>> SalesByDateAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);
        ;

        // Fetch WorkDays
        var workDays = await _context.WorkDays
            .Where(x => x.OrderHeaders.Any() && x.Date >= reportsRequest.From.Date && x.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
            .Select(x => x.Date).Distinct()
            .ToListAsync();

        // Fetch OrderHeaders
        var orderHeaders = await _context.OrderHeaders
            .Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
            .GroupBy(x => x.WorkDay.Date)
            .Select(g => new
            {
                Date = g.Key,
                SalesOrdersCount = g.Count(x => !x.IsReturn),
                SalesOrdersCountReturn = g.Count(x => x.IsReturn),
                CustomerCount = g.Count(x => x.CustomerId != null),
                GuestCount = g.Where(x => !x.IsReturn).Sum(x => x.GuestCount ?? 1)
            })
            .ToListAsync();

        // Fetch OrderItems
        var orderItems = await _context.OrderItems
            .Where(x => !x.Void && x.KotPrinted == true && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => x.OrderHeader.WorkDay.Date)
            .Select(g => new
            {
                Date = g.Key,
                ProductsBeforeDiscount = g.Where(x => !x.OrderHeader.IsReturn).Sum(x => (x.PriceVatInclusive == true ? x.Total + x.DiscountAmount - x.VatAmount : x.Total + x.DiscountAmount)),
                ProductsBeforeDiscountReturn = g.Where(x => x.OrderHeader.IsReturn).Sum(x => (x.PriceVatInclusive == true ? x.Total + x.DiscountAmount - x.VatAmount : x.Total + x.DiscountAmount)),
                ProductsDiscount = g.Where(x => !x.OrderHeader.IsReturn).Sum(x => x.DiscountAmount + x.HeaderDiscountAmount),
                ProductsDiscountReturn = g.Where(x => x.OrderHeader.IsReturn).Sum(x => x.DiscountAmount + x.HeaderDiscountAmount),
                ProductsTax = g.Where(x => !x.OrderHeader.IsReturn).Sum(x => x.VatAmount),
                ProductsTaxReturn = g.Where(x => x.OrderHeader.IsReturn).Sum(x => x.VatAmount),
                ProductsQuantity = g.Where(x => !x.OrderHeader.IsReturn).Sum(x => x.Quantity),
                ProductsQuantityReturn = g.Where(x => x.OrderHeader.IsReturn).Sum(x => x.Quantity)
            })
            .ToListAsync();

        // Fetch OrderItems for voided items
        var orderItemsVoid = await _context.OrderItems
            .Where(x => x.Void && x.KotPrinted == true && x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => x.OrderHeader.WorkDay.Date)
            .Select(g => new
            {
                Date = g.Key,
                ProductsVoidQuantity = g.Sum(z => z.Quantity),
                ProductsVoidAmount = g.Sum(z => z.Total)
            })
            .ToListAsync();

        // Fetch OrderFees
        var orderFees = await _context.OrderFees
            .Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => x.OrderHeader.WorkDay.Date)
            .Select(g => new
            {
                Date = g.Key,
                FeesBeforeDiscount = g.Sum(x => (x.PriceVatInclusive == true ? x.Total - x.VatAmount : x.Total)),
                FeesBeforeDiscountReturn = g.Sum(x => (x.PriceVatInclusive == true ? x.Total - x.VatAmount : x.Total)),
                FeesDiscount = 0m, // This is always zero in your current code
                FeesDiscountReturn = 0m, // This is always zero in your current code
                FeesTax = g.Sum(x => x.VatAmount),
                FeesTaxReturn = g.Sum(x => x.VatAmount)
            })
            .ToListAsync();

        // Join the data
        var result = from wd in workDays
                     join oh in orderHeaders on wd equals oh.Date into ohGroup
                     from oh in ohGroup.DefaultIfEmpty()
                     join oi in orderItems on wd equals oi.Date into oiGroup
                     from oi in oiGroup.DefaultIfEmpty()
                     join of in orderFees on wd equals of.Date into ofGroup
                     from of in ofGroup.DefaultIfEmpty()
                     join oiv in orderItemsVoid on wd equals oiv.Date into oivGroup
                     from oiv in oivGroup.DefaultIfEmpty()
                     select new SalesReportByDateModel()
                     {
                         Date = wd.ToString("yyyy-MM-dd"),
                         //OrdersCount = ((oh?.SalesOrdersCount ?? 0) - (oh?.SalesOrdersCountReturn ?? 0)).ToString("N0"),
                         //AverageOrder = (oh == null || oh.SalesOrdersCount == 0) ? "0" : Math.Round((((oi?.ProductsBeforeDiscount ?? 0) - (oi?.ProductsDiscount ?? 0) + (of?.FeesBeforeDiscount ?? 0) - (oi?.ProductsBeforeDiscountReturn ?? 0) + (oi?.ProductsDiscountReturn ?? 0) - (of?.FeesBeforeDiscountReturn ?? 0)) / oh.SalesOrdersCount), 2).ToString("N2"),
                         //AveragePerGuest = (oh == null || oh.GuestCount == 0) ? "0" : Math.Round((((oi?.ProductsBeforeDiscount ?? 0) - (oi?.ProductsDiscount ?? 0) + (of?.FeesBeforeDiscount ?? 0) - (oi?.ProductsBeforeDiscountReturn ?? 0) + (oi?.ProductsDiscountReturn ?? 0) - (of?.FeesBeforeDiscountReturn ?? 0)) / oh.GuestCount), 2).ToString("N2"),
                         //CustomersCount = oh?.CustomerCount.ToString("N0") ?? "0",
                         //GuestsCount = oh?.GuestCount.ToString("N0") ?? "0",
                         GrossSales = ((oi?.ProductsBeforeDiscount ?? 0) + (oi?.ProductsTax ?? 0) + (of?.FeesBeforeDiscount ?? 0) + (of?.FeesTax ?? 0) - (oi?.ProductsBeforeDiscountReturn ?? 0) - (oi?.ProductsTaxReturn ?? 0) - (of?.FeesBeforeDiscountReturn ?? 0) - (of?.FeesTaxReturn ?? 0)).ToString("N2"),
                         NetSales = ((oi?.ProductsBeforeDiscount ?? 0) - (oi?.ProductsDiscount ?? 0) + (of?.FeesBeforeDiscount ?? 0) - (oi?.ProductsBeforeDiscountReturn ?? 0) + (oi?.ProductsDiscountReturn ?? 0) - (of?.FeesBeforeDiscountReturn ?? 0)).ToString("N2"),
                         NetSalesWithTax = (((oi?.ProductsBeforeDiscount ?? 0) + (of?.FeesBeforeDiscount ?? 0) - (oi?.ProductsDiscount ?? 0) - (oi?.ProductsBeforeDiscountReturn ?? 0) + (oi?.ProductsDiscountReturn ?? 0) - (of?.FeesBeforeDiscountReturn ?? 0)) + ((oi?.ProductsTax ?? 0) + (of?.FeesTax ?? 0) - (oi?.ProductsTaxReturn ?? 0) - (of?.FeesTaxReturn ?? 0))).ToString("N2"),
                         NetQuantity = ((oi?.ProductsQuantity ?? 0) - (oi?.ProductsQuantityReturn ?? 0)).ToString("N2"),
                         VoidAmount = (oiv?.ProductsVoidAmount ?? 0).ToString("N2"),
                         VoidQuantity = (oiv?.ProductsVoidQuantity ?? 0).ToString("N2"),
                         DiscountAmount = ((oi?.ProductsDiscount ?? 0) + (of?.FeesDiscount ?? 0) - (oi?.ProductsDiscountReturn ?? 0) - (of?.FeesDiscountReturn ?? 0)).ToString("N2"),
                         VatAmount = ((oi?.ProductsTax ?? 0) + (of?.FeesTax ?? 0) - (oi?.ProductsTaxReturn ?? 0) - (of?.FeesTaxReturn ?? 0)).ToString("N2"),
                         RefundAmount = ((oi?.ProductsBeforeDiscountReturn ?? 0) + (of?.FeesBeforeDiscountReturn ?? 0)).ToString("N2"),
                         RefundQuantity = (oi?.ProductsQuantityReturn ?? 0).ToString("N2")
                     };

        return result;
    }

    private async Task<IEnumerable<SalesReportByXModel>> SalesByBranchAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
       //#1 Start With workday table
       await _context.OrderHeaders
       .Where
       (
           x =>
               x.OrderStatusId == "os-paid" &&
               x.VoidBy == null &&
               x.CreateAt.Date >= reportsRequest.From.Date &&
               x.CreateAt.Date <= reportsRequest.To.Date &&
               (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId))
        )
       .GroupBy(x => new { x.Branch.Id, x.Branch.Name, x.Branch.Sname }).Select(x => new
       {
           Id = x.Key.Id,
           Name = x.Key.Name,
           SName = x.Key.Sname,
           SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
           SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
           CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
           GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
       })
       .Select(c => new
       {
           Id = c.Id,
           Name = c.Name,
           SName = c.SName,
           SalesOrdersCount = c.SalesOrdersCount,
           SalesOrdersCountReturn = c.SalesOrdersCountReturn,
           CustomerCount = c.CustomerCount,
           GuestCount = c.GuestCount,
       })


       //#3Order detail table // sales only
       .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.CreateAt.Date >= reportsRequest.From.Date && x.OrderHeader.CreateAt.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
       .Select(oi => new
       {
           oi,
           oi.OrderHeader.WorkDay.Branch,
           oi.OrderHeader.IsReturn,
       })
       .GroupBy(x => new { x.Branch.Id, x.Branch.Name, x.Branch.Sname }).Select(x => new
       {
           Id = x.Key.Id,
           Name = x.Key.Name,
           SName = x.Key.Sname,
           ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
           ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
           ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
           ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
           ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
           ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
           ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
           ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
       }), c => c.Id, o => o.Id, (c, o) => new
       {
           c = c,
           o = o
       })
       .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
       {
           Id = c.c.Id,
           Name = c.c.Name,
           SName = c.c.SName,
           SalesOrdersCount = c.c.SalesOrdersCount,
           SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
           CustomerCount = c.c.CustomerCount,
           GuestCount = c.c.GuestCount,
           //Plus
           ProductsBeforeDiscount = (o == null) ? null : (decimal?)o.ProductsBeforeDiscount,
           ProductsBeforeDiscountReturn = (o == null) ? null : (decimal?)o.ProductsBeforeDiscountReturn,
           ProductsDiscount = (o == null) ? null : (decimal?)o.ProductsDiscount,
           ProductsDiscountReturn = (o == null) ? null : (decimal?)o.ProductsDiscountReturn,
           ProductsTax = (o == null) ? null : (decimal?)o.ProductsTax,
           ProductsTaxReturn = (o == null) ? null : (decimal?)o.ProductsTaxReturn,
           ProductsQuantity = (o == null) ? null : (decimal?)o.ProductsQuantity,
           ProductsQuantityReturn = (o == null) ? null : (decimal?)o.ProductsQuantityReturn
       })

       //#4 Sales Void
       .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.CreateAt.Date >= reportsRequest.From.Date && x.OrderHeader.CreateAt.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
       .GroupBy(x => new { x.OrderHeader.WorkDay.Branch.Id, x.OrderHeader.WorkDay.Branch.Name, x.OrderHeader.WorkDay.Branch.Sname }).Select(x => new
       {
           Id = x.Key.Id,
           Name = x.Key.Name,
           SName = x.Key.Sname,
           ProductsVoidQuantity = x.Sum(z => z.Quantity),
           ProductsVoidAmount = x.Sum(z => z.Total)
       }), c => c.Id, o => o.Id, (c, o) => new
       {
           c = c,
           o = o
       })
       .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
       {
           Id = c.c.Id,
           Name = c.c.Name,
           SName = c.c.SName,
           SalesOrdersCount = c.c.SalesOrdersCount,
           SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
           CustomerCount = c.c.CustomerCount,
           GuestCount = c.c.GuestCount,
           ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
           ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
           ProductsDiscount = c.c.ProductsDiscount,
           ProductsDiscountReturn = c.c.ProductsDiscountReturn,
           ProductsTax = c.c.ProductsTax,
           ProductsTaxReturn = c.c.ProductsTaxReturn,
           ProductsQuantity = c.c.ProductsQuantity,
           ProductsQuantityReturn = c.c.ProductsQuantityReturn,
           ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
           ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
       })

       //#5 Fees Details
       .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.CreateAt.Date >= reportsRequest.From.Date && x.OrderHeader.CreateAt.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.WorkDay.Branch,
              oi.OrderHeader.IsReturn
          })
          .GroupBy(x => new { x.Branch.Id, x.Branch.Name, x.Branch.Sname }).Select(x => new
          {
              Id = x.Key.Id,
              Name = x.Key.Name,
              SName = x.Key.Sname,
              FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
              FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
              FeesDiscount = 0,
              FeesDiscountReturn = 0,
              FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
              FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
          }), c => c.Id, o => o.Id, (c, o) => new
          {
              c = c,
              o = o
          })
       .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
       {
           Id = c.c.Id,
           Name = c.c.Name,
           SName = c.c.SName,
           SalesOrdersCount = c.c.SalesOrdersCount,
           SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
           CustomerCount = c.c.CustomerCount,
           GuestCount = c.c.GuestCount,
           ProductsBeforeDiscount = c.c.ProductsBeforeDiscount ?? 0,
           ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn ?? 0,
           ProductsDiscount = c.c.ProductsDiscount ?? 0,
           ProductsDiscountReturn = c.c.ProductsDiscountReturn ?? 0,
           ProductsTax = c.c.ProductsTax ?? 0,
           ProductsTaxReturn = c.c.ProductsTaxReturn ?? 0,
           ProductsQuantity = c.c.ProductsQuantity ?? 0,
           ProductsQuantityReturn = c.c.ProductsQuantityReturn ?? 0,
           ProductsVoidQuantity = c.c.ProductsVoidQuantity ?? 0,
           ProductsVoidAmount = c.c.ProductsVoidAmount ?? 0,
           FeesBeforeDiscount = ((o == null) ? null : (decimal?)o.FeesBeforeDiscount) ?? 0,
           FeesBeforeDiscountReturn = ((o == null) ? null : (decimal?)o.FeesBeforeDiscountReturn) ?? 0,
           FeesDiscount = ((o == null) ? null : (decimal?)o.FeesDiscount) ?? 0,
           FeesDiscountReturn = ((o == null) ? null : (decimal?)o.FeesDiscountReturn) ?? 0,
           FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
           FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
       })
           .Select(c => new SalesReportByXModel()
           {
               Id = c.Id,
               Name = c.Name,
               Sname = c.SName,
               //OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
               //AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
               //AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
               //CustomersCount = c.CustomerCount.ToString("N0"),
               //GuestsCount = c.GuestCount.ToString("N0"),
               GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
               NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
               NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
               NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
               VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
               VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
               DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
               VatAmount = ((c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
               RefundAmount = (c.ProductsBeforeDiscountReturn + c.FeesBeforeDiscountReturn).ToString("N2"),
               RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
           }).AsNoTracking().ToListAsync();

        return result;
    }

    private async Task<IEnumerable<SalesReportByXModel>> SalesByItemAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        // First, let's filter the OrderItems
        var filteredOrderItems = _context.OrderItems
            .Where(x => x.OrderHeader.OrderStatusId == "os-paid"
                && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date
                && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date);

        if (reportsRequest.BranchesIds.Any())
        {
            filteredOrderItems = filteredOrderItems
                .Where(x => reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId));
        }


        if (reportsRequest.ItemGroupsIds.Any())
        {
            filteredOrderItems = filteredOrderItems
                .Where(x => reportsRequest.ItemGroupsIds.Contains(x.Item.ItemGroupId));
        }

        if (reportsRequest.OrderSourcesIds.Any())
        {
            filteredOrderItems = filteredOrderItems
                .Where(x => reportsRequest.OrderSourcesIds.Contains(x.OrderHeader.OrderSourceId));
        }

        // Next, let's group and aggregate the order items
        var aggregatedOrderItems = await filteredOrderItems
            .Select(oi => new
            {
                oi,
                oi.OrderHeader.IsReturn
            })
            .GroupBy(x => new { x.oi.Item.Id, x.oi.Item.Name, x.oi.Item.Sname, VariantId = x.oi.Variant.Id, VariantName = x.oi.Variant.Name, VariantSname = x.oi.Variant.Sname }).Select(x => new
            {
                Id = x.Key.VariantId ?? x.Key.Id,
                Name = x.Key.VariantId == null ? x.Key.Name : x.Key.Name + " " + x.Key.VariantName,
                SName = x.Key.VariantId == null ? x.Key.Sname : x.Key.Sname + " " + x.Key.VariantSname,
                ProductsBeforeDiscount = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount)))),
                ProductsBeforeDiscountReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount)))),
                ProductsDiscount = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount))),
                ProductsDiscountReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount))),
                ProductsTax = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : z.oi.VatAmount)),
                ProductsTaxReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : z.oi.VatAmount)),
                ProductsQuantity = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : (z.oi.Modifier ? z.oi.Quantity * z.oi.ModifierParentNavigation.Quantity : z.oi.Quantity))),
                ProductsQuantityReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : (z.oi.Modifier ? z.oi.Quantity * z.oi.ModifierParentNavigation.Quantity : z.oi.Quantity))),
                ProductsVoidQuantity = x.Sum(z => (z.oi.Void && ((z.oi.KotPrinted ?? false) == true) && !z.IsReturn) ? (z.oi.Modifier ? z.oi.Quantity * z.oi.ModifierParentNavigation.Quantity : z.oi.Quantity) : 0),
                ProductsVoidAmount = x.Sum(z => (z.oi.Void && (z.oi.KotPrinted ?? false == true)) ? (z.oi.Total) : 0)
            }).Select(c => new SalesReportByXModel()
            {
                Id = c.Id,
                Name = c.Name,
                Sname = c.SName,
                GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn).ToString("N2"),
                NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn).ToString("N2"),
                NetSalesWithTax = ((c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn) + (c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
                NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                DiscountAmount = (c.ProductsDiscount - c.ProductsDiscountReturn).ToString("N2"),
                VatAmount = ((c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
                RefundAmount = (c.ProductsBeforeDiscountReturn).ToString("N2"),
                RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
            }).AsNoTracking().ToListAsync();

        return aggregatedOrderItems;
    }

    private async Task<IEnumerable<SalesReportByXModel>> SalesByModifierAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        // First, let's filter the OrderItems
        var filteredOrderItems = _context.OrderItems
            .Where(x => x.Modifier && x.OrderHeader.OrderStatusId == "os-paid"
                && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date
                && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date);

        // If branches is not "all", add another filter
        if (reportsRequest.BranchesIds.Any())
        {
            filteredOrderItems = filteredOrderItems
                .Where(x => reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId));
        }

        // If itemgroups is not "all", add another filter
        if (reportsRequest.ItemGroupsIds.Any())
        {
            filteredOrderItems = filteredOrderItems
                .Where(x => reportsRequest.ItemGroupsIds.Contains(x.Item.ItemGroupId));
        }

        // Next, let's group and aggregate the order items
        var aggregatedOrderItems = await filteredOrderItems
            .Select(oi => new
            {
                oi,
                oi.OrderHeader.IsReturn
            })
            .GroupBy(x => new { x.oi.Item.Id, x.oi.Item.Name, x.oi.Item.Sname, VariantId = x.oi.Variant.Id, VariantName = x.oi.Variant.Name, VariantSname = x.oi.Variant.Sname }).Select(x => new
            {
                Id = x.Key.VariantId ?? x.Key.Id,
                Name = x.Key.VariantId == null ? x.Key.Name : x.Key.Name + " " + x.Key.VariantName,
                SName = x.Key.VariantId == null ? x.Key.Sname : x.Key.Sname + " " + x.Key.VariantSname,
                ProductsBeforeDiscount = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount)))),
                ProductsBeforeDiscountReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount)))),
                ProductsDiscount = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount))),
                ProductsDiscountReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount))),
                ProductsTax = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : z.oi.VatAmount)),
                ProductsTaxReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : z.oi.VatAmount)),
                ProductsQuantity = x.Sum(z => (z.oi.Void) ? 0 : (z.IsReturn ? 0 : (z.oi.Modifier ? z.oi.Quantity * z.oi.ModifierParentNavigation.Quantity : z.oi.Quantity))),
                ProductsQuantityReturn = x.Sum(z => (z.oi.Void) ? 0 : (!z.IsReturn ? 0 : (z.oi.Modifier ? z.oi.Quantity * z.oi.ModifierParentNavigation.Quantity : z.oi.Quantity))),
                ProductsVoidQuantity = x.Sum(z => (z.oi.Void && ((z.oi.KotPrinted ?? false) == true) && !z.IsReturn) ? (z.oi.Modifier ? z.oi.Quantity * z.oi.ModifierParentNavigation.Quantity : z.oi.Quantity) : 0),
                //x.Sum(z => (z.oi.Void && (z.oi.KotPrinted ?? false == true)) ? ((z.oi.Modifier ? (z.oi.Price == 0 ? z.oi.Quantity : (z.oi.Total / z.oi.Price)) : z.oi.Quantity)) : 0),
                ProductsVoidAmount = x.Sum(z => (z.oi.Void && (z.oi.KotPrinted ?? false == true)) ? (z.oi.Total) : 0)
            }).Select(c => new SalesReportByXModel()
            {
                Id = c.Id,
                Name = c.Name,
                Sname = c.SName,
                GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn).ToString("N2"),
                NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn).ToString("N2"),
                NetSalesWithTax = ((c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn) + (c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
                NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                DiscountAmount = (c.ProductsDiscount - c.ProductsDiscountReturn).ToString("N2"),
                VatAmount = ((c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
                RefundAmount = (c.ProductsBeforeDiscountReturn).ToString("N2"),
                RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
            }).AsNoTracking().ToListAsync();


        //var result =
        // //#1 Start With workday table
        // await _context.OrderItems.Where(x => x.Modifier && !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
        // .Select(oi => new
        // {
        //	 oi,
        //	 oi.OrderHeader.IsReturn,
        // })
        // .GroupBy(x => new { x.oi.Item.Id, x.oi.Item.Name, x.oi.Item.Sname }).Select(x => new
        // {
        //	 Id = x.Key.Id,
        //	 Name = x.Key.Name,
        //	 SName = x.Key.Sname,
        //	 ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
        //	 ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
        //	 ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
        //	 ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
        //	 ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
        //	 ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
        //	 ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : (z.oi.Modifier ? (z.oi.Price == 0 ? z.oi.Quantity : (z.oi.Total / z.oi.Price)) : z.oi.Quantity)),
        //	 ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.Modifier ? (z.oi.Price == 0 ? z.oi.Quantity : (z.oi.Total / z.oi.Price)) : z.oi.Quantity)),
        // })
        //	//#4 Sales Void
        //	.GroupJoin(_context.OrderItems.Where(x => x.Modifier && x.Void && (x.KotPrinted ?? false == true) && !x.OrderHeader.IsReturn && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
        //	.GroupBy(x => new { x.Item.Id, x.Item.Name, x.Item.Sname }).Select(x => new
        //	{
        //		Id = x.Key.Id,
        //		Name = x.Key.Name,
        //		SName = x.Key.Sname,
        //		ProductsVoidQuantity = x.Sum(z => (z.Modifier ? (z.Price == 0 ? z.Quantity : (z.Total / z.Price)) : z.Quantity)),
        //		ProductsVoidAmount = x.Sum(z => z.Total)
        //	}), c => c.Id, o => o.Id, (c, o) => new
        //	{
        //		c = c,
        //		o = o
        //	})
        //	.SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
        //	{
        //		Id = c.c.Id,
        //		Name = c.c.Name,
        //		SName = c.c.SName,
        //		ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
        //		ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
        //		ProductsDiscount = c.c.ProductsDiscount,
        //		ProductsDiscountReturn = c.c.ProductsDiscountReturn,
        //		ProductsTax = c.c.ProductsTax,
        //		ProductsTaxReturn = c.c.ProductsTaxReturn,
        //		ProductsQuantity = c.c.ProductsQuantity,
        //		ProductsQuantityReturn = c.c.ProductsQuantityReturn,
        //		ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
        //		ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
        //	})
        //	.Select(c => new SalesReportByXModel()
        //	{
        //		Id = c.Id,
        //		Name = c.Name,
        //		Sname = c.SName,
        //		GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn).ToString("N2"),
        //		NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn).ToString("N2"),
        //		NetSalesWithTax = ((c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.ProductsDiscountReturn) + (c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
        //		NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
        //		VoidAmount = (c.ProductsVoidAmount ?? 0).ToString("N2"),
        //		VoidQuantity = (c.ProductsVoidQuantity ?? 0).ToString("N2"),
        //		DiscountAmount = (c.ProductsDiscount - c.ProductsDiscountReturn).ToString("N2"),
        //		VatAmount = ((c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
        //		RefundAmount = (c.ProductsBeforeDiscountReturn).ToString("N2"),
        //		RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
        //	}).AsNoTracking().ToListAsync();

        return aggregatedOrderItems;
    }

    private async Task<IEnumerable<SalesReportByXModel>> SalesByItemGroupAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
        //#1 Start With workday table
        await _context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
        .Select(oi => new
        {
            oi,
            oi.Item.ItemGroup,
            oi.OrderHeader.IsReturn,
        })
        .GroupBy(x => new { x.ItemGroup.Id, x.ItemGroup.Name, x.ItemGroup.Sname }).Select(x => new
        {
            Id = x.Key.Id,
            Name = x.Key.Name,
            SName = x.Key.Sname,
            ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
            ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
            ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
            ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
            ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
            ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
            ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
            ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
        })
           //#4 Sales Void
           .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && !x.OrderHeader.IsReturn && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
           .GroupBy(x => new { x.Item.ItemGroup.Id, x.Item.ItemGroup.Name, x.Item.ItemGroup.Sname }).Select(x => new
           {
               Id = x.Key.Id,
               Name = x.Key.Name,
               SName = x.Key.Sname,
               ProductsVoidQuantity = x.Sum(z => z.Quantity),
               ProductsVoidAmount = x.Sum(z => z.Total)
           }), c => c.Id, o => o.Id, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Id = c.c.Id,
               Name = c.c.Name,
               SName = c.c.SName,
               ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
               ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
               ProductsDiscount = c.c.ProductsDiscount,
               ProductsDiscountReturn = c.c.ProductsDiscountReturn,
               ProductsTax = c.c.ProductsTax,
               ProductsTaxReturn = c.c.ProductsTaxReturn,
               ProductsQuantity = c.c.ProductsQuantity,
               ProductsQuantityReturn = c.c.ProductsQuantityReturn,
               ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
               ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
           })
           .Select(c => new SalesReportByXModel()
           {
               Id = c.Id,
               Name = c.Name,
               Sname = c.SName,
               GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn).ToString("N2"),
               NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn).ToString("N2"),
               NetSalesWithTax = ((c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn) + (c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
               NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
               VoidAmount = (c.ProductsVoidAmount ?? 0).ToString("N2"),
               VoidQuantity = (c.ProductsVoidQuantity ?? 0).ToString("N2"),
               DiscountAmount = (c.ProductsDiscount - c.ProductsDiscountReturn).ToString("N2"),
               VatAmount = ((c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
               RefundAmount = (c.ProductsBeforeDiscountReturn).ToString("N2"),
               RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
           }).AsNoTracking().ToListAsync();

        return result;
    }

    private async Task<IEnumerable<SalesReportByXModel>> SalesByEmployeeAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
                //#1 Start With workday table
                await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
                .GroupBy(x => new { x.Waiter.Id, x.Waiter.Name, x.Waiter.Sname }).Select(x => new
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    SName = x.Key.Sname,
                    SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
                    SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
                    CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
                    GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
                })
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    SName = c.SName,
                    SalesOrdersCount = c.SalesOrdersCount,
                    SalesOrdersCountReturn = c.SalesOrdersCountReturn,
                    CustomerCount = c.CustomerCount,
                    GuestCount = c.GuestCount,
                })


                //#3Order detail table // sales only
                .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
                .Select(oi => new
                {
                    oi,
                    oi.OrderHeader.IsReturn,
                })
                .GroupBy(x => new { x.oi.OrderHeader.Waiter.Id, x.oi.OrderHeader.Waiter.Name, x.oi.OrderHeader.Waiter.Sname }).Select(x => new
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    SName = x.Key.Sname,
                    ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                    ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                    ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                    ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                    ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                    ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
                    ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
                    ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
                }), c => c.Id, o => o.Id, (c, o) => new
                {
                    c = c,
                    o = o
                })
                .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
                {
                    Id = c.c.Id,
                    Name = c.c.Name,
                    SName = c.c.SName,
                    SalesOrdersCount = c.c.SalesOrdersCount,
                    SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                    CustomerCount = c.c.CustomerCount,
                    GuestCount = c.c.GuestCount,
                    //Plus
                    ProductsBeforeDiscount = (o == null) ? null : (decimal?)o.ProductsBeforeDiscount,
                    ProductsBeforeDiscountReturn = (o == null) ? null : (decimal?)o.ProductsBeforeDiscountReturn,
                    ProductsDiscount = (o == null) ? null : (decimal?)o.ProductsDiscount,
                    ProductsDiscountReturn = (o == null) ? null : (decimal?)o.ProductsDiscountReturn,
                    ProductsTax = (o == null) ? null : (decimal?)o.ProductsTax,
                    ProductsTaxReturn = (o == null) ? null : (decimal?)o.ProductsTaxReturn,
                    ProductsQuantity = (o == null) ? null : (decimal?)o.ProductsQuantity,
                    ProductsQuantityReturn = (o == null) ? null : (decimal?)o.ProductsQuantityReturn
                })

                //#4 Sales Void
                .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
                .GroupBy(x => new { x.OrderHeader.Waiter.Id, x.OrderHeader.Waiter.Name, x.OrderHeader.Waiter.Sname }).Select(x => new
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    SName = x.Key.Sname,
                    ProductsVoidQuantity = x.Sum(z => z.Quantity),
                    ProductsVoidAmount = x.Sum(z => z.Total)
                }), c => c.Id, o => o.Id, (c, o) => new
                {
                    c = c,
                    o = o
                })
                .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
                {
                    Id = c.c.Id,
                    Name = c.c.Name,
                    SName = c.c.SName,
                    SalesOrdersCount = c.c.SalesOrdersCount,
                    SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                    CustomerCount = c.c.CustomerCount,
                    GuestCount = c.c.GuestCount,
                    ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
                    ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
                    ProductsDiscount = c.c.ProductsDiscount,
                    ProductsDiscountReturn = c.c.ProductsDiscountReturn,
                    ProductsTax = c.c.ProductsTax,
                    ProductsTaxReturn = c.c.ProductsTaxReturn,
                    ProductsQuantity = c.c.ProductsQuantity,
                    ProductsQuantityReturn = c.c.ProductsQuantityReturn,
                    ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
                    ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
                })

                //#5 Fees Details
                .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
                  .Select(oi => new
                  {
                      oi,
                      oi.OrderHeader.IsReturn
                  })
                  .GroupBy(x => new { x.oi.OrderHeader.Waiter.Id, x.oi.OrderHeader.Waiter.Name, x.oi.OrderHeader.Waiter.Sname }).Select(x => new
                  {
                      Id = x.Key.Id,
                      Name = x.Key.Name,
                      SName = x.Key.Sname,
                      FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                      FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                      FeesDiscount = 0,
                      FeesDiscountReturn = 0,
                      FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                      FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
                  }), c => c.Id, o => o.Id, (c, o) => new
                  {
                      c = c,
                      o = o
                  })
                .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
                {
                    Id = c.c.Id,
                    Name = c.c.Name,
                    SName = c.c.SName,
                    SalesOrdersCount = c.c.SalesOrdersCount,
                    SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                    CustomerCount = c.c.CustomerCount,
                    GuestCount = c.c.GuestCount,
                    ProductsBeforeDiscount = c.c.ProductsBeforeDiscount ?? 0,
                    ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn ?? 0,
                    ProductsDiscount = c.c.ProductsDiscount ?? 0,
                    ProductsDiscountReturn = c.c.ProductsDiscountReturn ?? 0,
                    ProductsTax = c.c.ProductsTax ?? 0,
                    ProductsTaxReturn = c.c.ProductsTaxReturn ?? 0,
                    ProductsQuantity = c.c.ProductsQuantity ?? 0,
                    ProductsQuantityReturn = c.c.ProductsQuantityReturn ?? 0,
                    ProductsVoidQuantity = c.c.ProductsVoidQuantity ?? 0,
                    ProductsVoidAmount = c.c.ProductsVoidAmount ?? 0,
                    FeesBeforeDiscount = ((o == null) ? null : (decimal?)o.FeesBeforeDiscount) ?? 0,
                    FeesBeforeDiscountReturn = ((o == null) ? null : (decimal?)o.FeesBeforeDiscountReturn) ?? 0,
                    FeesDiscount = ((o == null) ? null : (decimal?)o.FeesDiscount) ?? 0,
                    FeesDiscountReturn = ((o == null) ? null : (decimal?)o.FeesDiscountReturn) ?? 0,
                    FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
                    FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
                })
                   .Select(c => new SalesReportByXModel()
                   {
                       Id = c.Id,
                       Name = c.Name,
                       Sname = c.SName,
                       //OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                       //AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                       //AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                       //CustomersCount = c.CustomerCount.ToString("N0"),
                       //GuestsCount = c.GuestCount.ToString("N0"),
                       GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
                       NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
                       NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                       NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                       VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                       VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                       DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
                       VatAmount = ((c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                       RefundAmount = (c.ProductsBeforeDiscountReturn + c.FeesBeforeDiscountReturn).ToString("N2"),
                       RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
                   }).AsNoTracking().ToListAsync();

        return result;
    }

    private async Task<IEnumerable<PaymentTypeModel>> SalesByPaymentTypeAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        return await _context.OrderPayments
            .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => new { x.PaymentId, x.Payment.Name, x.Payment.Sname, x.OrderHeader.IsReturn })
            .Select(x => new
            {
                Id = x.Key.PaymentId,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                TotalAmount = x.Key.IsReturn ? 0 : x.Sum(x => x.Amount),
                RefundAmount = x.Key.IsReturn ? x.Sum(x => x.Amount) : 0,
                NetAmount = (x.Key.IsReturn ? 0 : x.Sum(x => x.Amount)) - (x.Key.IsReturn ? x.Sum(x => x.Amount) : 0)
            })
            .GroupBy(x => new { x.Id, x.Name, x.Sname })
            .Select(x => new PaymentTypeModel()
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                TotalAmount = x.Sum(c => c.TotalAmount).ToString("N2"),
                RefundAmount = x.Sum(c => c.RefundAmount).ToString("N2"),
                NetAmount = (x.Sum(c => c.TotalAmount) - x.Sum(c => c.RefundAmount)).ToString("N2")
            })
            .OrderBy(x => x.Id)
            .ToListAsync();
    }
    
    private async Task<IEnumerable<PaymentTypeByBranchModel>> SalesByPaymentTypeByBranchAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        return await _context.OrderPayments
            .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => new { x.PaymentId, x.Payment.Name, x.Payment.Sname, x.OrderHeader.BranchId, x.OrderHeader.IsReturn })
            .Select(x => new
            {
                Id = x.Key.PaymentId,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                BranchId = x.Key.BranchId,
                TotalAmount = x.Key.IsReturn ? 0 : x.Sum(x => x.Amount),
                RefundAmount = x.Key.IsReturn ? x.Sum(x => x.Amount) : 0,
                NetAmount = (x.Key.IsReturn ? 0 : x.Sum(x => x.Amount)) - (x.Key.IsReturn ? x.Sum(x => x.Amount) : 0)
            })
            .GroupBy(x => new { x.Id, x.Name, x.Sname, x.BranchId })
            .Select(x => new PaymentTypeByBranchModel()
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                BranchId = x.Key.BranchId,
                TotalAmount = x.Sum(c => c.TotalAmount).ToString("N2"),
                RefundAmount = x.Sum(c => c.RefundAmount).ToString("N2"),
                NetAmount = (x.Sum(c => c.TotalAmount) - x.Sum(c => c.RefundAmount)).ToString("N2")
            })
            .OrderBy(x => x.Id)
            .ThenBy(x => x.BranchId)
            .ToListAsync();
    }
    
    private async Task<IEnumerable<PaymentTypeByDateModel>> SalesByPaymentTypeByDateAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        return await _context.OrderPayments
            .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => new { x.PaymentId, x.Payment.Name, x.Payment.Sname, x.OrderHeader.WorkDay.Date, x.OrderHeader.IsReturn })
            .Select(x => new
            {
                Id = x.Key.PaymentId,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                Date = x.Key.Date,
                TotalAmount = x.Key.IsReturn ? 0 : x.Sum(x => x.Amount),
                RefundAmount = x.Key.IsReturn ? x.Sum(x => x.Amount) : 0,
                NetAmount = (x.Key.IsReturn ? 0 : x.Sum(x => x.Amount)) - (x.Key.IsReturn ? x.Sum(x => x.Amount) : 0)
            })
            .GroupBy(x => new { x.Id, x.Name, x.Sname, x.Date })
            .Select(x => new PaymentTypeByDateModel()
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                Date = x.Key.Date,
                TotalAmount = x.Sum(c => c.TotalAmount).ToString("N2"),
                RefundAmount = x.Sum(c => c.RefundAmount).ToString("N2"),
                NetAmount = (x.Sum(c => c.TotalAmount) - x.Sum(c => c.RefundAmount)).ToString("N2")
            })
            .OrderBy(x => x.Id)
            .ThenBy(x => x.Date)
            .ToListAsync();
    }
    
    private async Task<IEnumerable<PaymentTypeByBranchAndDateModel>> SalesByPaymentTypeByBranchAndDateAsync(ReportsRequest reportsRequest)
    {
        return await _context.OrderPayments
            .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => new { x.PaymentId, x.Payment.Name, x.Payment.Sname, x.OrderHeader.WorkDay.Date, x.OrderHeader.BranchId, x.OrderHeader.IsReturn })
            .Select(x => new
            {
                Id = x.Key.PaymentId,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                BranchId = x.Key.BranchId,
                Date = x.Key.Date,
                TotalAmount = x.Key.IsReturn ? 0 : x.Sum(x => x.Amount),
                RefundAmount = x.Key.IsReturn ? x.Sum(x => x.Amount) : 0,
                NetAmount = (x.Key.IsReturn ? 0 : x.Sum(x => x.Amount)) - (x.Key.IsReturn ? x.Sum(x => x.Amount) : 0)
            })
            .GroupBy(x => new { x.Id, x.Name, x.Sname, x.BranchId, x.Date })
            .Select(x => new PaymentTypeByBranchAndDateModel()
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                BranchId = x.Key.BranchId,
                Date = x.Key.Date,
                TotalAmount = x.Sum(c => c.TotalAmount).ToString("N2"),
                RefundAmount = x.Sum(c => c.RefundAmount).ToString("N2"),
                NetAmount = (x.Sum(c => c.TotalAmount) - x.Sum(c => c.RefundAmount)).ToString("N2")
            })
            .OrderBy(x => x.Id)
            .ThenBy(x => x.BranchId)
            .ThenBy(x => x.Date)
            .ToListAsync();
    }

    private async Task<IEnumerable<SalesReportByWorkDayModel>> SalesByWorkDayAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
          //#1 Start With workday table
          await _context.WorkDays.Where(x => /*x.OrderHeaders.Count() > 0 &&*/ x.Date >= reportsRequest.From.Date && x.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId))) // sales only
          .GroupBy(x => new { x.Id, x.Date, x.OpenAt, x.CloseAt, OpenBy = x.OpenByNavigation.Name, CloseBy = x.CloseByNavigation.Name, x.BranchId }).Select(x => new
          {
              Id = x.Key.Id,
              Date = x.Key.Date,
              OpenAt = x.Key.OpenAt,
              CloseAt = x.Key.CloseAt,
              OpenBy = x.Key.OpenBy,
              CloseBy = x.Key.CloseBy,
              BranchId = x.Key.BranchId
          })

          //#2 Sales Header 
          .GroupJoin(_context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
          .GroupBy(x => new { x.WorkDay.Id, x.WorkDay.Date, x.WorkDay.OpenAt, x.WorkDay.CloseAt, OpenBy = x.WorkDay.OpenByNavigation.Name, CloseBy = x.WorkDay.CloseByNavigation.Name, x.BranchId }).Select(x => new
          {
              Id = x.Key.Id,
              Date = x.Key.Date,
              OpenAt = x.Key.OpenAt,
              CloseAt = x.Key.CloseAt,
              OpenBy = x.Key.OpenBy,
              CloseBy = x.Key.CloseBy,
              BranchId = x.Key.BranchId,
              SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
              SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
              CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
              GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
          }), c => c.Id, o => o.Id, (c, o) => new
          {
              c = c,
              o = o
          })
          .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
          {
              Id = c.c.Id,
              Date = c.c.Date,
              OpenAt = c.c.OpenAt,
              CloseAt = c.c.CloseAt,
              OpenBy = c.c.OpenBy,
              CloseBy = c.c.CloseBy,
              BranchId = c.c.BranchId,
              SalesOrdersCount = (o == null) ? null : (decimal?)o.SalesOrdersCount,
              SalesOrdersCountReturn = (o == null) ? null : (decimal?)o.SalesOrdersCountReturn,
              CustomerCount = (o == null) ? null : (decimal?)o.CustomerCount,
              GuestCount = (o == null) ? null : (decimal?)o.GuestCount,
          })


          //#3Order detail table // sales only
          .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.WorkDay,
              oi.OrderHeader.IsReturn,
          })
          .GroupBy(x => new { x.WorkDay.Id, x.WorkDay.Date, x.WorkDay.OpenAt, x.WorkDay.CloseAt, OpenBy = x.WorkDay.OpenByNavigation.Name, CloseBy = x.WorkDay.CloseByNavigation.Name, x.WorkDay.BranchId }).Select(x => new
          {
              Id = x.Key.Id,
              Date = x.Key.Date,
              OpenAt = x.Key.OpenAt,
              CloseAt = x.Key.CloseAt,
              OpenBy = x.Key.OpenBy,
              CloseBy = x.Key.CloseBy,
              BranchId = x.Key.BranchId,
              ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
              ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
              ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
              ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
              ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
              ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
              ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
              ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
          }), c => c.Id, o => o.Id, (c, o) => new
          {
              c = c,
              o = o
          })
          .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
          {
              Id = c.c.Id,
              Date = c.c.Date,
              OpenAt = c.c.OpenAt,
              CloseAt = c.c.CloseAt,
              OpenBy = c.c.OpenBy,
              CloseBy = c.c.CloseBy,
              BranchId = c.c.BranchId,
              SalesOrdersCount = c.c.SalesOrdersCount,
              SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
              CustomerCount = c.c.CustomerCount,
              GuestCount = c.c.GuestCount,
              //Plus
              ProductsBeforeDiscount = (o == null) ? null : (decimal?)o.ProductsBeforeDiscount,
              ProductsBeforeDiscountReturn = (o == null) ? null : (decimal?)o.ProductsBeforeDiscountReturn,
              ProductsDiscount = (o == null) ? null : (decimal?)o.ProductsDiscount,
              ProductsDiscountReturn = (o == null) ? null : (decimal?)o.ProductsDiscountReturn,
              ProductsTax = (o == null) ? null : (decimal?)o.ProductsTax,
              ProductsTaxReturn = (o == null) ? null : (decimal?)o.ProductsTaxReturn,
              ProductsQuantity = (o == null) ? null : (decimal?)o.ProductsQuantity,
              ProductsQuantityReturn = (o == null) ? null : (decimal?)o.ProductsQuantityReturn
          })

          //#4 Sales Void
          .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
          .GroupBy(x => new { x.OrderHeader.WorkDay.Id, x.OrderHeader.WorkDay.Date, x.OrderHeader.WorkDay.OpenAt, x.OrderHeader.WorkDay.CloseAt, OpenBy = x.OrderHeader.WorkDay.OpenByNavigation.Name, CloseBy = x.OrderHeader.WorkDay.CloseByNavigation.Name, x.OrderHeader.WorkDay.BranchId }).Select(x => new
          {
              Id = x.Key.Id,
              Date = x.Key.Date,
              OpenAt = x.Key.OpenAt,
              CloseAt = x.Key.CloseAt,
              OpenBy = x.Key.OpenBy,
              CloseBy = x.Key.CloseBy,
              BranchId = x.Key.BranchId,
              ProductsVoidQuantity = x.Sum(z => z.Quantity),
              ProductsVoidAmount = x.Sum(z => z.Total)
          }), c => c.Id, o => o.Id, (c, o) => new
          {
              c = c,
              o = o
          })
          .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
          {
              Id = c.c.Id,
              Date = c.c.Date,
              OpenAt = c.c.OpenAt,
              CloseAt = c.c.CloseAt,
              OpenBy = c.c.OpenBy,
              CloseBy = c.c.CloseBy,
              BranchId = c.c.BranchId,
              SalesOrdersCount = c.c.SalesOrdersCount,
              SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
              CustomerCount = c.c.CustomerCount,
              GuestCount = c.c.GuestCount,
              ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
              ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
              ProductsDiscount = c.c.ProductsDiscount,
              ProductsDiscountReturn = c.c.ProductsDiscountReturn,
              ProductsTax = c.c.ProductsTax,
              ProductsTaxReturn = c.c.ProductsTaxReturn,
              ProductsQuantity = c.c.ProductsQuantity,
              ProductsQuantityReturn = c.c.ProductsQuantityReturn,
              ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
              ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
          })

          //#5 Fees Details
          .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
             .Select(oi => new
             {
                 oi,
                 oi.OrderHeader.WorkDay,
                 oi.OrderHeader.IsReturn
             })
          .GroupBy(x => new { x.WorkDay.Id, x.WorkDay.Date, x.WorkDay.OpenAt, x.WorkDay.CloseAt, OpenBy = x.WorkDay.OpenByNavigation.Name, CloseBy = x.WorkDay.CloseByNavigation.Name, x.WorkDay.BranchId }).Select(x => new
          {
              Id = x.Key.Id,
              Date = x.Key.Date,
              OpenAt = x.Key.OpenAt,
              CloseAt = x.Key.CloseAt,
              OpenBy = x.Key.OpenBy,
              CloseBy = x.Key.CloseBy,
              BranchId = x.Key.BranchId,
              FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
              FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
              FeesDiscount = 0,
              FeesDiscountReturn = 0,
              FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
              FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
          }), c => c.Id, o => o.Id, (c, o) => new
          {
              c = c,
              o = o
          })
          .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
          {
              Id = c.c.Id,
              Date = c.c.Date,
              OpenAt = c.c.OpenAt,
              CloseAt = c.c.CloseAt,
              OpenBy = c.c.OpenBy,
              CloseBy = c.c.CloseBy,
              BranchId = c.c.BranchId,
              SalesOrdersCount = c.c.SalesOrdersCount ?? 0,
              SalesOrdersCountReturn = c.c.SalesOrdersCountReturn ?? 0,
              CustomerCount = c.c.CustomerCount ?? 0,
              GuestCount = c.c.GuestCount ?? 0,
              ProductsBeforeDiscount = c.c.ProductsBeforeDiscount ?? 0,
              ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn ?? 0,
              ProductsDiscount = c.c.ProductsDiscount ?? 0,
              ProductsDiscountReturn = c.c.ProductsDiscountReturn ?? 0,
              ProductsTax = c.c.ProductsTax ?? 0,
              ProductsTaxReturn = c.c.ProductsTaxReturn ?? 0,
              ProductsQuantity = c.c.ProductsQuantity ?? 0,
              ProductsQuantityReturn = c.c.ProductsQuantityReturn ?? 0,
              ProductsVoidQuantity = c.c.ProductsVoidQuantity ?? 0,
              ProductsVoidAmount = c.c.ProductsVoidAmount ?? 0,
              FeesBeforeDiscount = ((o == null) ? null : (decimal?)o.FeesBeforeDiscount) ?? 0,
              FeesBeforeDiscountReturn = ((o == null) ? null : (decimal?)o.FeesBeforeDiscountReturn) ?? 0,
              FeesDiscount = ((o == null) ? null : (decimal?)o.FeesDiscount) ?? 0,
              FeesDiscountReturn = ((o == null) ? null : (decimal?)o.FeesDiscountReturn) ?? 0,
              FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
              FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
          })
              .Select(c => new SalesReportByWorkDayModel()
              {
                  Id = c.Id.ToString(),
                  BranchId = c.BranchId,
                  Date = c.Date.ToString("yyyy-MM-dd"),
                  OpenAt = c.OpenAt.ToString("hh:mm tt"),
                  CloseAt = c.CloseAt.Value.ToString("hh:mm tt"),
                  OpenBy = c.OpenBy,
                  CloseBy = c.CloseBy,
                  //OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                  //AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                  //AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                  //CustomersCount = c.CustomerCount.ToString("N0"),
                  //GuestsCount = c.GuestCount.ToString("N0"),
                  GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
                  NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
                  NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                  NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                  VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                  VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                  DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
                  VatAmount = ((c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                  RefundAmount = (c.ProductsBeforeDiscountReturn + c.FeesBeforeDiscountReturn).ToString("N2"),
                  RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
              }).AsNoTracking().ToListAsync();

        return result;
    }

    private async Task<IEnumerable<SalesReportByShiftModel>> SalesByShiftAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
              //#1 Start With workday table
              await _context.WorkDayShifts.Where(x => /*x.OrderHeaders.Count() > 0 &&*/ x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.WorkDay.BranchId))) // sales only
              .GroupBy(x => new { Id = x.Id, EId = x.Employee.Id, Date = x.WorkDay.Date, OpenAt = x.CreateAt, CloseAt = x.ClosedAt, Name = x.Employee.Name, Sname = x.Employee.Sname }).Select(x => new
              {
                  Id = x.Key.Id,
                  EId = x.Key.EId,
                  Date = x.Key.Date,
                  OpenAt = x.Key.OpenAt,
                  CloseAt = x.Key.CloseAt,
                  Name = x.Key.Name,
                  Sname = x.Key.Sname
              })

              //#2 Sales Header 
              .GroupJoin(_context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
              .GroupBy(x => new { Id = x.WorkDayShift.Id, EId = x.WorkDayShift.Employee.Id, Date = x.WorkDayShift.WorkDay.Date, OpenAt = x.WorkDayShift.CreateAt, CloseAt = x.WorkDayShift.ClosedAt, Name = x.WorkDayShift.Employee.Name, Sname = x.WorkDayShift.Employee.Sname }).Select(x => new
              {
                  Id = x.Key.Id,
                  EId = x.Key.EId,
                  Date = x.Key.Date,
                  OpenAt = x.Key.OpenAt,
                  CloseAt = x.Key.CloseAt,
                  Name = x.Key.Name,
                  Sname = x.Key.Sname,
                  SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
                  SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
                  CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
                  GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
              }), c => c.Id, o => o.Id, (c, o) => new
              {
                  c = c,
                  o = o
              })
              .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
              {
                  Id = c.c.Id,
                  EId = c.c.EId,
                  Date = c.c.Date,
                  OpenAt = c.c.OpenAt,
                  CloseAt = c.c.CloseAt,
                  Name = c.c.Name,
                  Sname = c.c.Sname,
                  SalesOrdersCount = (o == null) ? null : (decimal?)o.SalesOrdersCount,
                  SalesOrdersCountReturn = (o == null) ? null : (decimal?)o.SalesOrdersCountReturn,
                  CustomerCount = (o == null) ? null : (decimal?)o.CustomerCount,
                  GuestCount = (o == null) ? null : (decimal?)o.GuestCount,
              })


              //#3Order detail table // sales only
              .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
              .Select(oi => new
              {
                  oi,
                  oi.OrderHeader.WorkDayShift,
                  oi.OrderHeader.IsReturn,
              })
              .GroupBy(x => new { Id = x.WorkDayShift.Id, EId = x.WorkDayShift.Employee.Id, Date = x.WorkDayShift.WorkDay.Date, OpenAt = x.WorkDayShift.CreateAt, CloseAt = x.WorkDayShift.ClosedAt, Name = x.WorkDayShift.Employee.Name, Sname = x.WorkDayShift.Employee.Sname }).Select(x => new
              {
                  Id = x.Key.Id,
                  EId = x.Key.EId,
                  Date = x.Key.Date,
                  OpenAt = x.Key.OpenAt,
                  CloseAt = x.Key.CloseAt,
                  Name = x.Key.Name,
                  Sname = x.Key.Sname,
                  ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                  ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                  ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                  ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                  ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                  ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
                  ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
                  ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
              }), c => c.Id, o => o.Id, (c, o) => new
              {
                  c = c,
                  o = o
              })
              .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
              {
                  Id = c.c.Id,
                  EId = c.c.EId,
                  Date = c.c.Date,
                  OpenAt = c.c.OpenAt,
                  CloseAt = c.c.CloseAt,
                  Name = c.c.Name,
                  Sname = c.c.Sname,
                  SalesOrdersCount = c.c.SalesOrdersCount,
                  SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                  CustomerCount = c.c.CustomerCount,
                  GuestCount = c.c.GuestCount,
                  //Plus
                  ProductsBeforeDiscount = (o == null) ? null : (decimal?)o.ProductsBeforeDiscount,
                  ProductsBeforeDiscountReturn = (o == null) ? null : (decimal?)o.ProductsBeforeDiscountReturn,
                  ProductsDiscount = (o == null) ? null : (decimal?)o.ProductsDiscount,
                  ProductsDiscountReturn = (o == null) ? null : (decimal?)o.ProductsDiscountReturn,
                  ProductsTax = (o == null) ? null : (decimal?)o.ProductsTax,
                  ProductsTaxReturn = (o == null) ? null : (decimal?)o.ProductsTaxReturn,
                  ProductsQuantity = (o == null) ? null : (decimal?)o.ProductsQuantity,
                  ProductsQuantityReturn = (o == null) ? null : (decimal?)o.ProductsQuantityReturn
              })

              //#4 Sales Void
              .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
              .GroupBy(x => new { Id = x.OrderHeader.WorkDayShift.Id, EId = x.OrderHeader.WorkDayShift.Employee.Id, Date = x.OrderHeader.WorkDayShift.WorkDay.Date, OpenAt = x.OrderHeader.WorkDayShift.CreateAt, CloseAt = x.OrderHeader.WorkDayShift.ClosedAt, Name = x.OrderHeader.WorkDayShift.Employee.Name, Sname = x.OrderHeader.WorkDayShift.Employee.Sname }).Select(x => new
              {
                  Id = x.Key.Id,
                  EId = x.Key.EId,
                  Date = x.Key.Date,
                  OpenAt = x.Key.OpenAt,
                  CloseAt = x.Key.CloseAt,
                  Name = x.Key.Name,
                  Sname = x.Key.Sname,
                  ProductsVoidQuantity = x.Sum(z => z.Quantity),
                  ProductsVoidAmount = x.Sum(z => z.Total)
              }), c => c.Id, o => o.Id, (c, o) => new
              {
                  c = c,
                  o = o
              })
              .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
              {
                  Id = c.c.Id,
                  EId = c.c.EId,
                  Date = c.c.Date,
                  OpenAt = c.c.OpenAt,
                  CloseAt = c.c.CloseAt,
                  Name = c.c.Name,
                  Sname = c.c.Sname,
                  SalesOrdersCount = c.c.SalesOrdersCount,
                  SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                  CustomerCount = c.c.CustomerCount,
                  GuestCount = c.c.GuestCount,
                  ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
                  ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
                  ProductsDiscount = c.c.ProductsDiscount,
                  ProductsDiscountReturn = c.c.ProductsDiscountReturn,
                  ProductsTax = c.c.ProductsTax,
                  ProductsTaxReturn = c.c.ProductsTaxReturn,
                  ProductsQuantity = c.c.ProductsQuantity,
                  ProductsQuantityReturn = c.c.ProductsQuantityReturn,
                  ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
                  ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
              })

              //#5 Fees Details
              .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
                 .Select(oi => new
                 {
                     oi,
                     oi.OrderHeader.WorkDayShift,
                     oi.OrderHeader.IsReturn
                 })
              .GroupBy(x => new { Id = x.WorkDayShift.Id, EId = x.WorkDayShift.Employee.Id, Date = x.WorkDayShift.WorkDay.Date, OpenAt = x.WorkDayShift.CreateAt, CloseAt = x.WorkDayShift.ClosedAt, Name = x.WorkDayShift.Employee.Name, Sname = x.WorkDayShift.Employee.Sname }).Select(x => new
              {
                  Id = x.Key.Id,
                  EId = x.Key.EId,
                  Date = x.Key.Date,
                  OpenAt = x.Key.OpenAt,
                  CloseAt = x.Key.CloseAt,
                  Name = x.Key.Name,
                  Sname = x.Key.Sname,
                  FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                  FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                  FeesDiscount = 0,
                  FeesDiscountReturn = 0,
                  FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                  FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
              }), c => c.Id, o => o.Id, (c, o) => new
              {
                  c = c,
                  o = o
              })
              .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
              {
                  Id = c.c.Id,
                  EId = c.c.EId,
                  Date = c.c.Date,
                  OpenAt = c.c.OpenAt,
                  CloseAt = c.c.CloseAt,
                  Name = c.c.Name,
                  Sname = c.c.Sname,
                  SalesOrdersCount = c.c.SalesOrdersCount ?? 0,
                  SalesOrdersCountReturn = c.c.SalesOrdersCountReturn ?? 0,
                  CustomerCount = c.c.CustomerCount ?? 0,
                  GuestCount = c.c.GuestCount ?? 0,
                  ProductsBeforeDiscount = c.c.ProductsBeforeDiscount ?? 0,
                  ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn ?? 0,
                  ProductsDiscount = c.c.ProductsDiscount ?? 0,
                  ProductsDiscountReturn = c.c.ProductsDiscountReturn ?? 0,
                  ProductsTax = c.c.ProductsTax ?? 0,
                  ProductsTaxReturn = c.c.ProductsTaxReturn ?? 0,
                  ProductsQuantity = c.c.ProductsQuantity ?? 0,
                  ProductsQuantityReturn = c.c.ProductsQuantityReturn ?? 0,
                  ProductsVoidQuantity = c.c.ProductsVoidQuantity ?? 0,
                  ProductsVoidAmount = c.c.ProductsVoidAmount ?? 0,
                  FeesBeforeDiscount = ((o == null) ? null : (decimal?)o.FeesBeforeDiscount) ?? 0,
                  FeesBeforeDiscountReturn = ((o == null) ? null : (decimal?)o.FeesBeforeDiscountReturn) ?? 0,
                  FeesDiscount = ((o == null) ? null : (decimal?)o.FeesDiscount) ?? 0,
                  FeesDiscountReturn = ((o == null) ? null : (decimal?)o.FeesDiscountReturn) ?? 0,
                  FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
                  FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
              })
                  .Select(c => new SalesReportByShiftModel()
                  {
                      Id = c.Id.ToString(),
                      EmpId = c.EId,
                      Date = c.Date.ToString("yyyy-MM-dd"),
                      OpenedAt = c.OpenAt.ToString("hh:mm tt"),
                      ClosedAt = c.CloseAt.Value.AddHours(3).ToString("hh:mm tt"),
                      Name = c.Name,
                      Sname = c.Sname,
                      //OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                      //AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                      //AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                      //CustomersCount = c.CustomerCount.ToString("N0"),
                      //GuestsCount = c.GuestCount.ToString("N0"),
                      GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
                      NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
                      NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                      NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                      VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                      VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                      DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
                      VatAmount = ((c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                      RefundAmount = (c.ProductsBeforeDiscountReturn + c.FeesBeforeDiscountReturn).ToString("N2"),
                      RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
                  }).AsNoTracking().ToListAsync();

        return result;
    }

    private async Task<IEnumerable<SalesReportByXModel>> SalesByOrderSourceAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
            //#1 Start With workday table
            await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
            .GroupBy(x => new { x.OrderSource.Id, x.OrderSource.Name, x.OrderSource.Sname }).Select(x => new
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                SName = x.Key.Sname,
                SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
                SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
                CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
                GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
            })
            .Select(c => new
            {
                Id = c.Id,
                Name = c.Name,
                SName = c.SName,
                SalesOrdersCount = c.SalesOrdersCount,
                SalesOrdersCountReturn = c.SalesOrdersCountReturn,
                CustomerCount = c.CustomerCount,
                GuestCount = c.GuestCount,
            })


            //#3Order detail table // sales only
            .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .Select(oi => new
            {
                oi,
                oi.OrderHeader.OrderSource,
                oi.OrderHeader.IsReturn,
            })
            .GroupBy(x => new { x.OrderSource.Id, x.OrderSource.Name, x.OrderSource.Sname }).Select(x => new
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                SName = x.Key.Sname,
                ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
                ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
                ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
            }), c => c.Id, o => o.Id, (c, o) => new
            {
                c = c,
                o = o
            })
            .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
            {
                Id = c.c.Id,
                Name = c.c.Name,
                SName = c.c.SName,
                SalesOrdersCount = c.c.SalesOrdersCount,
                SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                CustomerCount = c.c.CustomerCount,
                GuestCount = c.c.GuestCount,
                //Plus
                ProductsBeforeDiscount = (o == null) ? null : (decimal?)o.ProductsBeforeDiscount,
                ProductsBeforeDiscountReturn = (o == null) ? null : (decimal?)o.ProductsBeforeDiscountReturn,
                ProductsDiscount = (o == null) ? null : (decimal?)o.ProductsDiscount,
                ProductsDiscountReturn = (o == null) ? null : (decimal?)o.ProductsDiscountReturn,
                ProductsTax = (o == null) ? null : (decimal?)o.ProductsTax,
                ProductsTaxReturn = (o == null) ? null : (decimal?)o.ProductsTaxReturn,
                ProductsQuantity = (o == null) ? null : (decimal?)o.ProductsQuantity,
                ProductsQuantityReturn = (o == null) ? null : (decimal?)o.ProductsQuantityReturn
            })

            //#4 Sales Void
            .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => new { x.OrderHeader.OrderSource.Id, x.OrderHeader.OrderSource.Name, x.OrderHeader.OrderSource.Sname }).Select(x => new
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                SName = x.Key.Sname,
                ProductsVoidQuantity = x.Sum(z => z.Quantity),
                ProductsVoidAmount = x.Sum(z => z.Total)
            }), c => c.Id, o => o.Id, (c, o) => new
            {
                c = c,
                o = o
            })
            .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
            {
                Id = c.c.Id,
                Name = c.c.Name,
                SName = c.c.SName,
                SalesOrdersCount = c.c.SalesOrdersCount,
                SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                CustomerCount = c.c.CustomerCount,
                GuestCount = c.c.GuestCount,
                ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
                ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
                ProductsDiscount = c.c.ProductsDiscount,
                ProductsDiscountReturn = c.c.ProductsDiscountReturn,
                ProductsTax = c.c.ProductsTax,
                ProductsTaxReturn = c.c.ProductsTaxReturn,
                ProductsQuantity = c.c.ProductsQuantity,
                ProductsQuantityReturn = c.c.ProductsQuantityReturn,
                ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
                ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
            })

            //#5 Fees Details
            .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
              .Select(oi => new
              {
                  oi,
                  oi.OrderHeader.OrderSource,
                  oi.OrderHeader.IsReturn
              })
              .GroupBy(x => new { x.OrderSource.Id, x.OrderSource.Name, x.OrderSource.Sname }).Select(x => new
              {
                  Id = x.Key.Id,
                  Name = x.Key.Name,
                  SName = x.Key.Sname,
                  FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                  FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                  FeesDiscount = 0,
                  FeesDiscountReturn = 0,
                  FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                  FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
              }), c => c.Id, o => o.Id, (c, o) => new
              {
                  c = c,
                  o = o
              })
            .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
            {
                Id = c.c.Id,
                Name = c.c.Name,
                SName = c.c.SName,
                SalesOrdersCount = c.c.SalesOrdersCount,
                SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
                CustomerCount = c.c.CustomerCount,
                GuestCount = c.c.GuestCount,
                ProductsBeforeDiscount = c.c.ProductsBeforeDiscount ?? 0,
                ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn ?? 0,
                ProductsDiscount = c.c.ProductsDiscount ?? 0,
                ProductsDiscountReturn = c.c.ProductsDiscountReturn ?? 0,
                ProductsTax = c.c.ProductsTax ?? 0,
                ProductsTaxReturn = c.c.ProductsTaxReturn ?? 0,
                ProductsQuantity = c.c.ProductsQuantity ?? 0,
                ProductsQuantityReturn = c.c.ProductsQuantityReturn ?? 0,
                ProductsVoidQuantity = c.c.ProductsVoidQuantity ?? 0,
                ProductsVoidAmount = c.c.ProductsVoidAmount ?? 0,
                FeesBeforeDiscount = ((o == null) ? null : (decimal?)o.FeesBeforeDiscount) ?? 0,
                FeesBeforeDiscountReturn = ((o == null) ? null : (decimal?)o.FeesBeforeDiscountReturn) ?? 0,
                FeesDiscount = ((o == null) ? null : (decimal?)o.FeesDiscount) ?? 0,
                FeesDiscountReturn = ((o == null) ? null : (decimal?)o.FeesDiscountReturn) ?? 0,
                FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
                FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
            })
               .Select(c => new SalesReportByXModel()
               {
                   Id = c.Id,
                   Name = c.Name,
                   Sname = c.SName,
                   //OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                   //AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                   //AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                   //CustomersCount = c.CustomerCount.ToString("N0"),
                   //GuestsCount = c.GuestCount.ToString("N0"),
                   GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
                   NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
                   NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                   NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                   VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                   VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                   DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
                   VatAmount = ((c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                   RefundAmount = (c.ProductsBeforeDiscountReturn + c.FeesBeforeDiscountReturn).ToString("N2"),
                   RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
               }).AsNoTracking().ToListAsync();

        return result;
        //var result =
        //   //#1 Start With workday table
        //   await _context.OrderItems.Where(x => x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
        //   .GroupBy(x => new { x.OrderHeader.OrderSource.Id, x.OrderHeader.OrderSource.Name, x.OrderHeader.OrderSource.Sname }).Select(x => new
        //   {
        //       Id = x.Key.Id,
        //       Name = x.Key.Name,
        //       SName = x.Key.Sname
        //   })

        //   //#3Order detail table // sales only
        //   .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && !x.OrderHeader.IsReturn && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
        //   .GroupBy(x => new { x.OrderHeader.OrderSource.Id, x.OrderHeader.OrderSource.Name, x.OrderHeader.OrderSource.Sname }).Select(x => new
        //   {
        //       Id = x.Key.Id,
        //       Name = x.Key.Name,
        //       SName = x.Key.Sname,
        //       GrossSales = x.Sum(c => ((c.PriceVatInclusive ?? false) ? c.Total : c.Total + c.VatAmount) + c.DiscountAmount /*+ c.HeaderDiscountAmount*/),
        //       NetSales = x.Sum(c => ((c.PriceVatInclusive ?? false) ? c.Total - c.VatAmount : c.Total) /*- c.DiscountAmount*/ - c.HeaderDiscountAmount),
        //       NetSalesWithTax = x.Sum(c => ((c.PriceVatInclusive ?? false) ? c.Total : c.Total + c.VatAmount) /*- c.DiscountAmount*/ - c.HeaderDiscountAmount),
        //       NetQuantity = x.Sum(c => c.Quantity),
        //       DiscountAmount = x.Sum(c => c.DiscountAmount + c.HeaderDiscountAmount),
        //       VatAmount = x.Sum(c => c.VatAmount)
        //   }), c => c.Id, o => o.Id, (c, o) => new
        //   {
        //       c = c,
        //       o = o
        //   })
        //   .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
        //   {
        //       Id = c.c.Id,
        //       Name = c.c.Name,
        //       SName = c.c.SName,
        //       GrossSales = (o == null) ? null : (decimal?)o.GrossSales,
        //       NetSales = (o == null) ? null : (decimal?)o.NetSales,
        //       NetSalesWithTax = (o == null) ? null : (decimal?)o.NetSalesWithTax,
        //       NetQuantity = (o == null) ? null : (decimal?)o.NetQuantity,
        //       DiscountAmount = (o == null) ? null : (decimal?)o.DiscountAmount,
        //       VatAmount = (o == null) ? null : (decimal?)o.VatAmount
        //   })

        //   //#4 detail voided
        //   .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
        //   .GroupBy(x => new { x.OrderHeader.OrderSource.Id, x.OrderHeader.OrderSource.Name, x.OrderHeader.OrderSource.Sname }).Select(x => new
        //   {
        //       Id = x.Key.Id,
        //       Name = x.Key.Name,
        //       SName = x.Key.Sname,
        //       VoidAmount = x.Sum(c => c.Total),
        //       VoidQuantity = x.Sum(c => c.Quantity)
        //   }), c => c.Id, o => o.Id, (c, o) => new
        //   {
        //       c = c,
        //       o = o
        //   })
        //   .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
        //   {
        //       Id = c.c.Id,
        //       Name = c.c.Name,
        //       SName = c.c.SName,
        //       GrossSales = c.c.GrossSales,
        //       NetSales = c.c.NetSales,
        //       NetSalesWithTax = c.c.NetSalesWithTax,
        //       NetQuantity = c.c.NetQuantity,
        //       DiscountAmount = c.c.DiscountAmount,
        //       VatAmount = c.c.VatAmount,
        //       VoidAmount = (o == null) ? null : (decimal?)o.VoidAmount,
        //       VoidQuantity = (o == null) ? null : (decimal?)o.VoidQuantity,
        //   })


        //   //#5 detail return
        //   .GroupJoin(_context.OrderItems.Where(x => x.OrderHeader.IsReturn && !x.Void && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
        //   .GroupBy(x => new { x.OrderHeader.OrderSource.Id, x.OrderHeader.OrderSource.Name, x.OrderHeader.OrderSource.Sname }).Select(x => new
        //   {
        //       Id = x.Key.Id,
        //       Name = x.Key.Name,
        //       SName = x.Key.Sname,
        //       RefundAmount = x.Sum(c => c.Total),
        //       RefundQuantity = x.Sum(c => c.Quantity)
        //   }), c => c.Id, o => o.Id, (c, o) => new
        //   {
        //       c = c,
        //       o = o
        //   })
        //   .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new SalesReportByXModel()
        //   {
        //       Id = c.c.Id,
        //       Name = c.c.Name,
        //       Sname = c.c.SName,
        //       GrossSales = (c.c.GrossSales ?? 0).ToString("N2"),
        //       NetSales = (c.c.NetSales ?? 0).ToString("N2"),
        //       NetSalesWithTax = (c.c.NetSalesWithTax ?? 0).ToString("N2"),
        //       NetQuantity = (c.c.NetQuantity ?? 0).ToString("N0"),
        //       VoidAmount = (c.c.VoidAmount ?? 0).ToString("N2"),
        //       VoidQuantity = (c.c.VoidQuantity ?? 0).ToString("N0"),
        //       DiscountAmount = (c.c.DiscountAmount ?? 0).ToString("N2"),
        //       VatAmount = (c.c.VatAmount ?? 0).ToString("N2"),
        //       RefundAmount = (((o == null) ? null : (decimal?)o.RefundAmount) ?? 0).ToString("N0"),
        //       RefundQuantity = (((o == null) ? null : (decimal?)o.RefundQuantity) ?? 0).ToString("N0")
        //   }).AsNoTracking().ToListAsync();

        //return Ok(result);
    }

    private async Task<IEnumerable<SalesReportByXModel>> SalesByDiningOptionAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
          //#1 Start With workday table
          await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
          .GroupBy(x => new { x.DiningOption.Id, x.DiningOption.Name, x.DiningOption.Sname }).Select(x => new
          {
              Id = x.Key.Id,
              Name = x.Key.Name,
              SName = x.Key.Sname,
              SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
              SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
              CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
              GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
          })
          .Select(c => new
          {
              Id = c.Id,
              Name = c.Name,
              SName = c.SName,
              SalesOrdersCount = c.SalesOrdersCount,
              SalesOrdersCountReturn = c.SalesOrdersCountReturn,
              CustomerCount = c.CustomerCount,
              GuestCount = c.GuestCount,
          })


          //#3Order detail table // sales only
          .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.DiningOption,
              oi.OrderHeader.IsReturn,
          })
          .GroupBy(x => new { x.DiningOption.Id, x.DiningOption.Name, x.DiningOption.Sname }).Select(x => new
          {
              Id = x.Key.Id,
              Name = x.Key.Name,
              SName = x.Key.Sname,
              ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
              ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
              ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
              ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
              ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
              ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
              ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
              ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
          }), c => c.Id, o => o.Id, (c, o) => new
          {
              c = c,
              o = o
          })
          .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
          {
              Id = c.c.Id,
              Name = c.c.Name,
              SName = c.c.SName,
              SalesOrdersCount = c.c.SalesOrdersCount,
              SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
              CustomerCount = c.c.CustomerCount,
              GuestCount = c.c.GuestCount,
              //Plus
              ProductsBeforeDiscount = (o == null) ? null : (decimal?)o.ProductsBeforeDiscount,
              ProductsBeforeDiscountReturn = (o == null) ? null : (decimal?)o.ProductsBeforeDiscountReturn,
              ProductsDiscount = (o == null) ? null : (decimal?)o.ProductsDiscount,
              ProductsDiscountReturn = (o == null) ? null : (decimal?)o.ProductsDiscountReturn,
              ProductsTax = (o == null) ? null : (decimal?)o.ProductsTax,
              ProductsTaxReturn = (o == null) ? null : (decimal?)o.ProductsTaxReturn,
              ProductsQuantity = (o == null) ? null : (decimal?)o.ProductsQuantity,
              ProductsQuantityReturn = (o == null) ? null : (decimal?)o.ProductsQuantityReturn
          })

          //#4 Sales Void
          .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
          .GroupBy(x => new { x.OrderHeader.DiningOption.Id, x.OrderHeader.DiningOption.Name, x.OrderHeader.DiningOption.Sname }).Select(x => new
          {
              Id = x.Key.Id,
              Name = x.Key.Name,
              SName = x.Key.Sname,
              ProductsVoidQuantity = x.Sum(z => z.Quantity),
              ProductsVoidAmount = x.Sum(z => z.Total)
          }), c => c.Id, o => o.Id, (c, o) => new
          {
              c = c,
              o = o
          })
          .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
          {
              Id = c.c.Id,
              Name = c.c.Name,
              SName = c.c.SName,
              SalesOrdersCount = c.c.SalesOrdersCount,
              SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
              CustomerCount = c.c.CustomerCount,
              GuestCount = c.c.GuestCount,
              ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
              ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
              ProductsDiscount = c.c.ProductsDiscount,
              ProductsDiscountReturn = c.c.ProductsDiscountReturn,
              ProductsTax = c.c.ProductsTax,
              ProductsTaxReturn = c.c.ProductsTaxReturn,
              ProductsQuantity = c.c.ProductsQuantity,
              ProductsQuantityReturn = c.c.ProductsQuantityReturn,
              ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
              ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
          })

          //#5 Fees Details
          .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
            .Select(oi => new
            {
                oi,
                oi.OrderHeader.DiningOption,
                oi.OrderHeader.IsReturn
            })
            .GroupBy(x => new { x.DiningOption.Id, x.DiningOption.Name, x.DiningOption.Sname }).Select(x => new
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                SName = x.Key.Sname,
                FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                FeesDiscount = 0,
                FeesDiscountReturn = 0,
                FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
            }), c => c.Id, o => o.Id, (c, o) => new
            {
                c = c,
                o = o
            })
          .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
          {
              Id = c.c.Id,
              Name = c.c.Name,
              SName = c.c.SName,
              SalesOrdersCount = c.c.SalesOrdersCount,
              SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
              CustomerCount = c.c.CustomerCount,
              GuestCount = c.c.GuestCount,
              ProductsBeforeDiscount = c.c.ProductsBeforeDiscount ?? 0,
              ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn ?? 0,
              ProductsDiscount = c.c.ProductsDiscount ?? 0,
              ProductsDiscountReturn = c.c.ProductsDiscountReturn ?? 0,
              ProductsTax = c.c.ProductsTax ?? 0,
              ProductsTaxReturn = c.c.ProductsTaxReturn ?? 0,
              ProductsQuantity = c.c.ProductsQuantity ?? 0,
              ProductsQuantityReturn = c.c.ProductsQuantityReturn ?? 0,
              ProductsVoidQuantity = c.c.ProductsVoidQuantity ?? 0,
              ProductsVoidAmount = c.c.ProductsVoidAmount ?? 0,
              FeesBeforeDiscount = ((o == null) ? null : (decimal?)o.FeesBeforeDiscount) ?? 0,
              FeesBeforeDiscountReturn = ((o == null) ? null : (decimal?)o.FeesBeforeDiscountReturn) ?? 0,
              FeesDiscount = ((o == null) ? null : (decimal?)o.FeesDiscount) ?? 0,
              FeesDiscountReturn = ((o == null) ? null : (decimal?)o.FeesDiscountReturn) ?? 0,
              FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
              FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
          })
             .Select(c => new SalesReportByXModel()
             {
                 Id = c.Id,
                 Name = c.Name,
                 Sname = c.SName,
                 //OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                 //AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                 //AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                 //CustomersCount = c.CustomerCount.ToString("N0"),
                 //GuestsCount = c.GuestCount.ToString("N0"),
                 GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
                 NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
                 NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                 NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                 VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                 VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                 DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
                 VatAmount = ((c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                 RefundAmount = (c.ProductsBeforeDiscountReturn + c.FeesBeforeDiscountReturn).ToString("N2"),
                 RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
             }).AsNoTracking().ToListAsync();

        return result;
    }

    private async Task<IEnumerable<SalesReportByHourModel>> SalesByHourAsync(ReportsRequest reportsRequest)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
               //#1 Start With workday table
               //await _context.WorkDays.Where(x => x.OrderHeaders.Count() > 0 && x.Date >= from.Date && x.Date <= to.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId))) // sales only
               await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
            .GroupBy(x => new { x.CreateAt.Hour }).Select(x => new
            {
                Hour = x.Key.Hour
            })

           //return Ok(result);

           //#2 Sales Header 
           .GroupJoin(_context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= reportsRequest.From.Date && x.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.BranchId)))
           .GroupBy(x => new { x.CreateAt.Hour }).Select(x => new
           {
               Hour = x.Key.Hour,
               SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
               SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
               CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
               GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
           }), c => c.Hour, o => o.Hour, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Hour = c.c.Hour,
               SalesOrdersCount = (o == null) ? null : (decimal?)o.SalesOrdersCount,
               SalesOrdersCountReturn = (o == null) ? null : (decimal?)o.SalesOrdersCountReturn,
               CustomerCount = (o == null) ? null : (decimal?)o.CustomerCount,
               GuestCount = (o == null) ? null : (decimal?)o.GuestCount,
           })


           //#3Order detail table // sales only
           .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
           .Select(oi => new
           {
               oi,
               oi.OrderHeader.CreateAt.Hour,
               oi.OrderHeader.IsReturn,
           })
           .GroupBy(x => new { x.Hour }).Select(x => new
           {
               Hour = x.Key.Hour,
               ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
               ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
               ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
               ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
               ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
               ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
               ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
               ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
           }), c => c.Hour, o => o.Hour, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Hour = c.c.Hour,
               SalesOrdersCount = c.c.SalesOrdersCount,
               SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
               CustomerCount = c.c.CustomerCount,
               GuestCount = c.c.GuestCount,
               //Plus
               ProductsBeforeDiscount = (o == null) ? null : (decimal?)o.ProductsBeforeDiscount,
               ProductsBeforeDiscountReturn = (o == null) ? null : (decimal?)o.ProductsBeforeDiscountReturn,
               ProductsDiscount = (o == null) ? null : (decimal?)o.ProductsDiscount,
               ProductsDiscountReturn = (o == null) ? null : (decimal?)o.ProductsDiscountReturn,
               ProductsTax = (o == null) ? null : (decimal?)o.ProductsTax,
               ProductsTaxReturn = (o == null) ? null : (decimal?)o.ProductsTaxReturn,
               ProductsQuantity = (o == null) ? null : (decimal?)o.ProductsQuantity,
               ProductsQuantityReturn = (o == null) ? null : (decimal?)o.ProductsQuantityReturn
           })

           //#4 Sales Void
           .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
           .GroupBy(x => new { x.OrderHeader.CreateAt.Hour }).Select(x => new
           {
               Hour = x.Key.Hour,
               ProductsVoidQuantity = x.Sum(z => z.Quantity),
               ProductsVoidAmount = x.Sum(z => z.Total)
           }), c => c.Hour, o => o.Hour, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Hour = c.c.Hour,
               SalesOrdersCount = c.c.SalesOrdersCount,
               SalesOrdersCountReturn = c.c.SalesOrdersCountReturn,
               CustomerCount = c.c.CustomerCount,
               GuestCount = c.c.GuestCount,
               ProductsBeforeDiscount = c.c.ProductsBeforeDiscount,
               ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn,
               ProductsDiscount = c.c.ProductsDiscount,
               ProductsDiscountReturn = c.c.ProductsDiscountReturn,
               ProductsTax = c.c.ProductsTax,
               ProductsTaxReturn = c.c.ProductsTaxReturn,
               ProductsQuantity = c.c.ProductsQuantity,
               ProductsQuantityReturn = c.c.ProductsQuantityReturn,
               ProductsVoidQuantity = (o == null) ? null : (decimal?)o.ProductsVoidQuantity,
               ProductsVoidAmount = (o == null) ? null : (decimal?)o.ProductsVoidAmount
           })

           //#5 Fees Details
           .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= reportsRequest.From.Date && x.OrderHeader.WorkDay.Date <= reportsRequest.To.Date && (!reportsRequest.BranchesIds.Any() ? true : reportsRequest.BranchesIds.Contains(x.OrderHeader.WorkDay.BranchId)))
              .Select(oi => new
              {
                  oi,
                  oi.OrderHeader.CreateAt.Hour,
                  oi.OrderHeader.IsReturn
              })
           .GroupBy(x => new { x.Hour }).Select(x => new
           {
               Hour = x.Key.Hour,
               FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
               FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
               FeesDiscount = 0,
               FeesDiscountReturn = 0,
               FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
               FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
           }), c => c.Hour, o => o.Hour, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Hour = c.c.Hour,
               SalesOrdersCount = c.c.SalesOrdersCount ?? 0,
               SalesOrdersCountReturn = c.c.SalesOrdersCountReturn ?? 0,
               CustomerCount = c.c.CustomerCount ?? 0,
               GuestCount = c.c.GuestCount ?? 0,
               ProductsBeforeDiscount = c.c.ProductsBeforeDiscount ?? 0,
               ProductsBeforeDiscountReturn = c.c.ProductsBeforeDiscountReturn ?? 0,
               ProductsDiscount = c.c.ProductsDiscount ?? 0,
               ProductsDiscountReturn = c.c.ProductsDiscountReturn ?? 0,
               ProductsTax = c.c.ProductsTax ?? 0,
               ProductsTaxReturn = c.c.ProductsTaxReturn ?? 0,
               ProductsQuantity = c.c.ProductsQuantity ?? 0,
               ProductsQuantityReturn = c.c.ProductsQuantityReturn ?? 0,
               ProductsVoidQuantity = c.c.ProductsVoidQuantity ?? 0,
               ProductsVoidAmount = c.c.ProductsVoidAmount ?? 0,
               FeesBeforeDiscount = ((o == null) ? null : (decimal?)o.FeesBeforeDiscount) ?? 0,
               FeesBeforeDiscountReturn = ((o == null) ? null : (decimal?)o.FeesBeforeDiscountReturn) ?? 0,
               FeesDiscount = ((o == null) ? null : (decimal?)o.FeesDiscount) ?? 0,
               FeesDiscountReturn = ((o == null) ? null : (decimal?)o.FeesDiscountReturn) ?? 0,
               FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
               FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
           })
               .Select(c => new SalesReportByHourModel()
               {
                   Hour = TimeSpan.FromHours(c.Hour).ToString("hh"),
                   //OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                   //AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                   //AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                   //CustomersCount = c.CustomerCount.ToString("N0"),
                   //GuestsCount = c.GuestCount.ToString("N0"),
                   GrossSales = (c.ProductsBeforeDiscount + c.ProductsTax + c.FeesBeforeDiscount + c.FeesTax - c.ProductsBeforeDiscountReturn - c.ProductsTaxReturn - c.FeesBeforeDiscountReturn - c.FeesTaxReturn).ToString("N2"),
                   NetSales = (c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn).ToString("N2"),
                   NetSalesWithTax = ((c.ProductsBeforeDiscount + c.FeesBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn) + (c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                   NetQuantity = (c.ProductsQuantity - c.ProductsQuantityReturn).ToString("N2"),
                   VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                   VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                   DiscountAmount = (c.ProductsDiscount + c.FeesDiscount - c.ProductsDiscountReturn - c.FeesDiscountReturn).ToString("N2"),
                   VatAmount = ((c.ProductsTax + c.FeesTax - c.ProductsTaxReturn - c.FeesTaxReturn)).ToString("N2"),
                   RefundAmount = (c.ProductsBeforeDiscountReturn + c.FeesBeforeDiscountReturn).ToString("N2"),
                   RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
               }).AsNoTracking().ToListAsync();

        return result;
    }
}
