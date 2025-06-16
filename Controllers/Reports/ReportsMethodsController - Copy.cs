using System.Diagnostics;
using System.Linq.Expressions;
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
    [HttpGet("SalesByDiscount")] //return discount model
    public async Task<IActionResult> SalesByDiscountAsync(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var hd =
                //#1 Start With workday table
                await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.DiscountId != null && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId)))
                .GroupBy(x => new { Id = x.DiscountId, x.Discount.Name, x.Discount.Sname }).Select(x => new
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    Sname = x.Key.Sname,
                    HeaderDiscount = x.Sum(x => !x.IsReturn ? x.HeaderDiscountAmount : 0),
                    HeaderDiscountReturn = x.Sum(x => x.IsReturn ? x.HeaderDiscountAmount : 0),

                    DetailDiscount = (decimal)0,
                    DetailDiscountReturn = (decimal)0,
                }).AsNoTracking().ToListAsync();


        var dd =
                await _context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.DiscountId != null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
                .Select(oi => new
                {
                    oi,
                    oi.OrderHeader.IsReturn
                })
                .GroupBy(x => new { Id = x.oi.DiscountId, x.oi.Discount.Name, x.oi.Discount.Sname }).Select(x => new
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    Sname = x.Key.Sname,
                    HeaderDiscount = (decimal)0,
                    HeaderDiscountReturn = (decimal)0,
                    DetailDiscount = x.Sum(z => !z.IsReturn ? z.oi.DiscountAmount : 0),
                    DetailDiscountReturn = x.Sum(z => z.IsReturn ? z.oi.DiscountAmount : 0),
                }).AsNoTracking().ToListAsync();

        var result = hd.Union(dd).GroupBy(x => new { x.Id, x.Name, x.Sname }).Select(x => new SalesReportByDiscountModel
        {
            Id = x.Key.Id,
            Name = x.Key.Name,
            Sname = x.Key.Sname,
            TotalAmount = (x.Sum(x => x.HeaderDiscount - x.HeaderDiscountReturn) + x.Sum(x => x.DetailDiscount - x.DetailDiscountReturn)).ToString("N2"),
            OrderLevelAmount = x.Sum(x => x.HeaderDiscount - x.HeaderDiscountReturn).ToString("N2"),
            LineLevelAmount = x.Sum(x => x.DetailDiscount - x.DetailDiscountReturn).ToString("N2")
        }).ToList();

        return Ok(result);

        // var hd = await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.DiscountId != null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.WorkDay.BranchId))) // sales only
        // .GroupBy(x => new { x.DiscountId, x.Discount.Name, x.Discount.Sname }).Select(x => new
        // {
        //     Id = x.Key.DiscountId,
        //     Name = x.Key.Name,
        //     Sname = x.Key.Sname,
        //     OrderLevelAmount = x.Sum(x => x.HeaderDiscountAmount),
        //     LineLevelAmount = (decimal)0
        // }).AsNoTracking().ToListAsync();

        // var dd = await _context.OrderItems.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.DiscountId != null && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId))) // sales only
        //.GroupBy(x => new { x.DiscountId, x.Discount.Name, x.Discount.Sname }).Select(x => new
        //{
        //    Id = x.Key.DiscountId,
        //    Name = x.Key.Name,
        //    Sname = x.Key.Sname,
        //    OrderLevelAmount = (decimal)0,
        //    LineLevelAmount = x.Sum(x => x.DiscountAmount)
        //}).AsNoTracking().ToListAsync();

        // var result = hd.Union(dd).GroupBy(x => new { x.Id, x.Name, x.Sname }).Select(x => new SalesReportByDiscountModel
        // {
        //     Id = x.Key.Id,
        //     Name = x.Key.Name,
        //     Sname = x.Key.Sname,
        //     TotalAmount = (x.Sum(x => x.OrderLevelAmount) + x.Sum(x => x.LineLevelAmount)).ToString("N2"),
        //     OrderLevelAmount = x.Sum(x => x.OrderLevelAmount).ToString("N2"),
        //     LineLevelAmount = x.Sum(x => x.LineLevelAmount).ToString("N2")
        // }).ToList();

        // return Ok(result);
        //if (branches == "all" || branches == null)
        //{
        //    var result = await _context.OrderHeaders.Where(x => x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date)
        //        .SelectMany(x => x.OrderItems).GroupBy(x => new { x.Discount.Id, x.Discount.Name, x.Discount.Sname }).Select(x => new SalesReportModel()
        //        {
        //            Id = x.Key.Id,
        //            Name = x.Key.Name,
        //            Sname = x.Key.Sname,
        //            CustomersCount = x.Count().ToString("N0"),
        //            OrdersCount = x.Count().ToString("N0"),
        //            NetQuantity = x.Sum(c => c.Quantity).ToString("N0"),
        //            DiscountAmount = Math.Round(x.Sum(c => c.DiscountAmount), 2).ToString("N"),
        //        }).AsNoTracking().ToListAsync();

        //    return Ok(result);
        //}
        //else
        //{
        //    List<string> branchesList = branches.Split(',').ToList<string>();
        //    var result = await _context.OrderHeaders.Where(x => x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && branchesList.Contains(x.BranchId))
        //        .SelectMany(x => x.OrderItems).GroupBy(x => new { x.Discount.Id, x.Discount.Name, x.Discount.Sname }).Select(x => new SalesReportModel()
        //        {
        //            Id = x.Key.Id,
        //            Name = x.Key.Name,
        //            Sname = x.Key.Sname,
        //            CustomersCount = x.Count().ToString("N0"),
        //            OrdersCount = x.Count().ToString("N0"),
        //            NetQuantity = x.Sum(c => c.Quantity).ToString("N0"),
        //            DiscountAmount = Math.Round(x.Sum(c => c.DiscountAmount), 2).ToString("N"),
        //        }).AsNoTracking().ToListAsync();

        //    return Ok(result);
        //}

    }

    [HttpGet("SalesByVatGroup")] // return vatgroup model
    public async Task<IActionResult> SalesByVatGroupAsync(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
            //#1 Start With workday table
            await _context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.VatGroupId != null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
            .Select(oi => new
            {
                oi,
                oi.OrderHeader.IsReturn,
            })
            .GroupBy(x => new { x.oi.VatGroup.Id, x.oi.VatGroup.Name, x.oi.VatGroup.Sname }).Select(x => new
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
            })

            //#5 Fees Details
            .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
              .Select(of => new
              {
                  of,
                  of.OrderHeader.IsReturn
              })
              .GroupBy(x => new { Id = x.of.VatGroupId }).Select(x => new
              {
                  Id = x.Key.Id,
                  FeesTax = x.Sum(z => z.IsReturn ? 0 : z.of.VatAmount),
                  FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.of.VatAmount)
              }), c => c.Id, o => o.Id, (c, o) => new
              {
                  c = c,
                  o = o
              })
            .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
            {
                Id = c.c.Id,
                Name = c.c.Name,
                Sname = c.c.Sname,
                ProductsTax = c.c.ProductsTax,
                ProductsTaxReturn = c.c.ProductsTaxReturn,
                FeesTax = ((o == null) ? null : (decimal?)o.FeesTax) ?? 0,
                FeesTaxReturn = ((o == null) ? null : (decimal?)o.FeesTaxReturn) ?? 0
            })
                .Select(c => new SalesReportByVatGroupModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Sname = c.Sname,
                    TotalAmount = (c.ProductsTax - c.ProductsTaxReturn + c.FeesTax - c.FeesTaxReturn).ToString("N2"),
                }).AsNoTracking().ToListAsync();

        return Ok(result);
    }

    [HttpGet("SalesByReceipt")] // return by receipt model
    public async Task<IActionResult> SalesByReceiptAsync(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result = await _context.OrderHeaders.Include(x => x.OrderPayments).ThenInclude(x => x.Payment).Where(x => x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.WorkDay.BranchId))) // sales only
       .Select(x => new SalesReportByReceiptModel
       {
           Id = x.Id,
           Date = x.WorkDay.Date.ToString("yyyy-MM-dd"),
           Branch = new IdNameModel { Id = x.WorkDay.Branch.Id, Name = x.WorkDay.Branch.Name, Sname = x.WorkDay.Branch.Sname },
           Number = x.OrderNumber.ToString(),
           Time = x.CreateAt.ToString("hh:mm tt"),
           DiningOption = new IdNameModel { Id = x.DiningOption.Id, Name = x.DiningOption.Name, Sname = x.DiningOption.Sname },
           OrderSource = new IdNameModel { Id = x.OrderSource.Id, Name = x.OrderSource.Name, Sname = x.OrderSource.Sname },
           Type = new IdNameModel { Id = x.IsReturn ? "1" : "0", Name = x.IsReturn ? "Retrun" : "Sales", Sname = x.IsReturn ? "مرتجع" : "مبيعات" },
           //TotalVAT = x.TotalVat,
           //TotalFees = x.FeesTotal,  

           //LineTotal  = x.LineTotal,
           //LineLevelDiscountAmount = x.OrderItems.Where(x=>!x.Void).Sum(x=>x.DiscountAmount),
           //OrderLevelDiscountAmount = x.HeaderDiscountAmount,
           Total = x.Total.ToString("N2"),

           CreatedBy = new IdNameModel { Id = x.CreateByNavigation.Id, Name = x.CreateByNavigation.Name, Sname = x.CreateByNavigation.Sname },
           WaiterName = new IdNameModel { Id = x.Waiter.Id, Name = x.Waiter.Name, Sname = x.Waiter.Sname },
           Cashair = new IdNameModel { Id = x.PaidByNavigation.Id, Name = x.PaidByNavigation.Name, Sname = x.PaidByNavigation.Sname },
           Customer = new CustomerModel { Id = x.Customer.Id.ToString(), Name = x.Customer.Name, Phone = x.Customer.Phone },
           OrderPayments = x.OrderPayments
       }).AsNoTracking().ToListAsync();

        return Ok(result);
    }

    [HttpGet("TobacoReport")] // return by receipt model
    public async Task<IActionResult> TobacoReport(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result = await _context.OrderHeaders.Include(x => x.OrderPayments).ThenInclude(x => x.Payment)
                                                .Include(x => x.OrderItems)
                                                .Include(x => x.OrderFees)
                                                .Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.WorkDay.BranchId))) // sales only

           .Select(x => new SalesReportByReceiptModel
           {
               Id = x.Id,
               Date = x.WorkDay.Date.ToString("yyyy-MM-dd"),
               Branch = new IdNameModel { Id = x.WorkDay.Branch.Id, Name = x.WorkDay.Branch.Name, Sname = x.WorkDay.Branch.Sname },
               Number = x.OrderNumber.ToString(),
               InvoiceNumber = x.InvoiceNumber.ToString(),
               Time = x.CreateAt.ToString("hh:mm tt"),
               DiningOption = new IdNameModel { Id = x.DiningOption.Id, Name = x.DiningOption.Name, Sname = x.DiningOption.Sname },
               OrderSource = new IdNameModel { Id = x.OrderSource.Id, Name = x.OrderSource.Name, Sname = x.OrderSource.Sname },
               Type = new IdNameModel { Id = x.IsReturn ? "1" : "0", Name = x.IsReturn ? "Retrun" : "Sales", Sname = x.IsReturn ? "مرتجع" : "مبيعات" },
               SubTotal = x.LineTotal.ToString("N2"),
               TotalVat = x.TotalVat.ToString("N2"),
               TotalFees = x.FeesTotal.ToString("N2"),
               Total = x.Total.ToString("N2"),
               CreatedBy = new IdNameModel { Id = x.CreateByNavigation.Id, Name = x.CreateByNavigation.Name, Sname = x.CreateByNavigation.Sname },
               WaiterName = new IdNameModel { Id = x.Waiter.Id, Name = x.Waiter.Name, Sname = x.Waiter.Sname },
               Cashair = new IdNameModel { Id = x.PaidByNavigation.Id, Name = x.PaidByNavigation.Name, Sname = x.PaidByNavigation.Sname },
               Customer = new CustomerModel { Id = x.Customer.Id.ToString(), Name = x.Customer.Name, Phone = x.Customer.Phone },
               OrderPayments = x.OrderPayments
           }).AsNoTracking().ToListAsync();

        return Ok(result);
    }

    [HttpPost("ZatcaReport")] // return by zatca model
    public async Task<IActionResult> ZatcaReport(DatatableRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var query = _context.OrderHeaders.Include(x => x.OrderPayments).ThenInclude(x => x.Payment)
                                         .Where(x => x.DeviceId != null &&
                                         x.OrderStatusId == "os-paid" &&
                                         x.VatReportStatus != null &&
                                         x.WorkDay.OpenAt.Date >= request.From.Value.Date &&
                                         x.WorkDay.OpenAt.Date <= request.To.Value.Date &&
                                         (request.Branches == "all" ? true : request.Branches.Contains(x.WorkDay.BranchId)))
                                         .OrderByDescending(x => x.CreateAt)
                                         .AsNoTracking();


        if (!string.IsNullOrEmpty(request.Search?.Value))
        {
            query = query.Where(c => c.OrderNumber.ToString().Contains(request.Search.Value) ||
                                     c.ZatcaReportStatus.Contains(request.Search.Value) ||
                                     c.VatReportStatus.Contains(request.Search.Value) ||
                                     c.CreateAt.Date.ToString().Contains(request.Search.Value) ||
                                     c.CreateAt.TimeOfDay.ToString().Contains(request.Search.Value) ||
                                     c.InvoiceNumber.Contains(request.Search.Value));
        }

        // Get the total number of records before pagination
        var totalRecords = await query.CountAsync();

        // Apply pagination
        query = query.Skip(request.Start).Take(request.Length);

        // Execute the query and select the desired data
        var reports = await query.Select(x => new ZatcaReport
        {
            Id = x.InvoiceNumber,
            UUID = x.Id,
            OrderNumber = x.OrderNumber,
            IsReturn = x.IsReturn,
            Date = x.CreateAt.Date.ToString("yyyy-MM-dd"),
            Time = x.CreateAt.ToString("hh:mm tt"),
            Branch = new IdNameModel { Id = x.WorkDay.Branch.Id, Name = x.WorkDay.Branch.Name, Sname = x.WorkDay.Branch.Sname },
            Total = x.Total.ToString("N2"),
            Customer = new CustomerModel { Id = x.Customer.Id.ToString(), Name = x.Customer.Name, Phone = x.Customer.Phone },
            OrderPayments = x.OrderPayments,
            VatReportStatus = x.VatReportStatus,
            ZatcaReportStatus = x.ZatcaReportStatus
        })
        .AsNoTracking()
        .ToListAsync();

        // Return the data with pagination info
        return Ok(new
        {
            Draw = request.Draw,//* Provide the correct draw parameter if using DataTables or similar */,
            RecordsTotal = totalRecords,
            RecordsFiltered = totalRecords, // This might change if you implement additional filters
            Data = reports
        });
    }

    [HttpGet("SalesByHour")] // return by receipt model
    public async Task<IActionResult> SalesByHourAsync(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
               //#1 Start With workday table
               //await _context.WorkDays.Where(x => x.OrderHeaders.Count() > 0 && x.Date >= from.Date && x.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId))) // sales only
               await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId)))
            .GroupBy(x => new { x.CreateAt.Hour }).Select(x => new
            {
                Hour = x.Key.Hour
            })

           //return Ok(result);

           //#2 Sales Header 
           .GroupJoin(_context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId)))
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
           .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
           .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
           .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
               .Select(c => new SalesReportByXModel()
               {
                   Id = c.Hour.ToString(),
                   Name = TimeSpan.FromHours(c.Hour).ToString("hh"),
                   Sname = TimeSpan.FromHours(c.Hour).ToString("hh"),
                   OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                   AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                   AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                   CustomersCount = c.CustomerCount.ToString("N0"),
                   GuestsCount = c.GuestCount.ToString("N0"),
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


        List<HoursOfDay> hoursOfDayList = new List<HoursOfDay>() {
        new HoursOfDay(){Hour = 0, Name = "12 AM", Sname = "" },
        new HoursOfDay(){Hour = 1, Name = "1 AM", Sname = "" },
        new HoursOfDay(){Hour = 2, Name = "2 AM", Sname = "" },
        new HoursOfDay(){Hour = 3, Name = "3 AM", Sname = "" },
        new HoursOfDay(){Hour = 4, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 5, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 6, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 7, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 8, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 9, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 10, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 11, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 12, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 13, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 14, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 15, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 16, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 17, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 18, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 19, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 20, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 21, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 22, Name = "", Sname = "" },
        new HoursOfDay(){Hour = 23, Name = "", Sname = "" },
        };


        return Ok(result);
    }

    [HttpGet("SalesByFee")]
    public async Task<IActionResult> SalesByFee(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result = await _context.OrderFees
            .Where(x => x.OrderHeader.OrderStatusId == "os-paid" &&
                        x.OrderHeader.WorkDay.Date >= from.Date &&
                        x.OrderHeader.WorkDay.Date <= to.Date &&
                        (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
            .Select(of => new
            {
                of,
                of.OrderHeader.IsReturn
            })
            .GroupBy(x => new { x.of.Fee.Id, x.of.Fee.Name, x.of.Fee.Sname })
            .Select(x => new
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                Sname = x.Key.Sname,
                FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.of.PriceVatInclusive ?? false) == true ? (z.of.Total - z.of.VatAmount) : (z.of.Total))),
                FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.of.PriceVatInclusive ?? false) == true ? (z.of.Total - z.of.VatAmount) : (z.of.Total))),
                FeesDiscount = 0,
                FeesDiscountReturn = 0,
                FeesTax = x.Sum(z => z.IsReturn ? 0 : z.of.VatAmount),
                FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.of.VatAmount)
            })
            .Select(c => new SalesReportByFeeModel
            {
                Id = c.Id,
                Name = c.Name,
                Sname = c.Sname,
                TotalAmount = (c.FeesBeforeDiscount - c.FeesBeforeDiscountReturn).ToString("N2"),
                TotalVat = (c.FeesTax - c.FeesTaxReturn).ToString("N2")
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost("SalesByCustomerDiscount")]
    public async Task<IActionResult> SalesByCustomerDiscount([FromBody] DatatableAPIRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        try
        {
            // var stopwatch = Stopwatch.StartNew(); // Start timing

            var customersCount = _context.Customers
                .Where(c => (string.IsNullOrEmpty(request.Search.Value) ||
                    c.Name.Contains(request.Search.Value) ||
                    c.Phone.Contains(request.Search.Value))
                    &&
                    (request.CustomerGroups == null ||
                    c.CustomerCustomerGroups.Any(cg => request.CustomerGroups.Contains(cg.CustomerGroupId)))
                    )
                .AsNoTracking()
                .Count();

            var customers = _context.Customers
                .Include(c => c.CreateByNavigation)
                 .Where(c => (string.IsNullOrEmpty(request.Search.Value) ||
                    c.Name.Contains(request.Search.Value) ||
                    c.Phone.Contains(request.Search.Value)) &&
                    (request.CustomerGroups == null ||
                    c.CustomerCustomerGroups.Any(cg => request.CustomerGroups.Contains(cg.CustomerGroupId)))
                    )
                .AsNoTracking()
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            var customerIds = customers
                .Where(c => c.Id != null)
                .Select(c => c.Id)
                .ToList();

            var orderHeader = _context.OrderHeaders
                .Where(oh => request.Discounts.Contains(oh.DiscountId)
                    && oh.CreateAt.Date >= request.From.Value.Date && oh.CreateAt.Date <= request.To.Value.Date
                    && oh.CustomerId != null
                    && customerIds.Contains(oh.CustomerId.Value))
                .GroupBy(oh => oh.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalDiscount = g.Sum(oh => oh.HeaderDiscountAmount),
                    TotalSpent = g.Sum(oh => oh.Total)
                })
                .OrderByDescending(x => x.TotalDiscount)
                .ToList();

            var orderItem = _context.OrderItems
                .Where(oi => request.Discounts.Contains(oi.DiscountId)
                    && oi.OrderHeader.CustomerId != null
                    && customerIds.Contains(oi.OrderHeader.CustomerId.Value))
                .GroupBy(oi => oi.OrderHeader.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalDiscount = g.Sum(oi => oi.DiscountAmount),
                    TotalSpent = g.Sum(oi => oi.Total),
                }).ToList();

            var unionQuery = orderHeader
                .Union(orderItem);

            var finalQuery = unionQuery
                .GroupBy(u => u.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key!,
                    TotalDiscount = g.Sum(u => u.TotalDiscount),
                    TotalSpent = g.Sum(u => u.TotalSpent),
                })
                .ToList();


            var customerReports = customers
                .GroupJoin(
                    finalQuery,
                    c => c.Id,               // Key selector from the `customers` query
                    g => g.CustomerId,       // Key selector from the `finalQuery`
                    (c, g) => new { Customer = c, FinalData = g.DefaultIfEmpty() } // Perform the left join
                )
                .SelectMany(
                    x => x.FinalData, // Flatten the results (including nulls for unmatched rows)
                    (x, g) => new CustomerReport
                    {
                        Phone = x.Customer.Phone,
                        Name = x.Customer.Name,
                        Points = x.Customer.Points,
                        TotalVisits = x.Customer.OrderHeaders.Count(),
                        FirstVisit = x.Customer.FirstVisit,
                        LastVisit = x.Customer.LastVisit,
                        CreatedByName = x.Customer?.CreateByNavigation.Name,
                        CreatedBySname = x.Customer?.CreateByNavigation.Sname,
                        CreateAt = x.Customer.CreateAt,
                        TotalSpent = g?.TotalSpent ?? 0, // Handle nulls for unmatched rows
                        TotalDiscount = g?.TotalDiscount ?? 0 // Handle nulls for unmatched rows
                    }
                )

                .OrderByDescending(x => x.TotalDiscount)
                .ToList();

            return Ok(new DatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = customersCount,
                RecordsFiltered = customersCount,
                Data = customerReports
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("ApiOrders")]
    public async Task<IActionResult> ApiOrders([FromBody] DatatableAPIRequest request)
    {
        if (request.Branches == null)
        {
            request.Branches = new HashSet<string>();
        }

        if (request.From == null || request.To == null)
        {
            throw new Exception();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);


        var stagingStatus = await _context.ApiOrderStagingStatuses
            .AsNoTracking()
            .ToListAsync();

        int apiOrdersCount = await _context.ApiOrders
            .Where
            (
                ao =>
                    (string.IsNullOrEmpty(request.Search.Value) ? true : ao.AppOrderNumber.Contains(request.Search.Value)) &&
                    ao.AppOrderReceiveDatetime >= request.From.Value.Date &&
                    ao.AppOrderReceiveDatetime <= request.To.Value.Date &&
                    (request.Branches.Any() ? request.Branches.Contains(ao.BranchId) : true)
            )
            .AsNoTracking()
            .CountAsync();

        List<ApiOrder> joinedApiOrders = [];

        if (apiOrdersCount > 0)
        {
            IQueryable<ApiOrder> apiOrdersQuery = _context.ApiOrders
                .Where
                (
                    ao =>
                        (string.IsNullOrEmpty(request.Search.Value) ? true : ao.AppOrderNumber.Contains(request.Search.Value)) &&
                        ao.AppOrderReceiveDatetime >= request.From.Value.Date &&
                        ao.AppOrderReceiveDatetime <= request.To.Value.Date &&
                        (request.Branches.Any() ? request.Branches.Contains(ao.BranchId) : true)
                )
                .AsNoTracking();

            if (request.Order.Count > 0)
            {
                for (int i = 0; i < request.Order.Count; i++)
                {
                    var columnName = request.Columns[request.Order[i].Column].Data;
                    var parameter = Expression.Parameter(typeof(ApiOrder), "c");
                    var property = Expression.Property(parameter, columnName);
                    var convertedProperty = Expression.Convert(property, typeof(object));
                    var lambda = Expression.Lambda<Func<ApiOrder, object>>(convertedProperty, parameter);

                    if (i == 0)
                    {
                        if (request.Order[i].Dir == "asc")
                        {
                            apiOrdersQuery = Queryable.OrderBy(apiOrdersQuery, lambda);
                        }
                        else
                        {
                            apiOrdersQuery = Queryable.OrderByDescending(apiOrdersQuery, lambda);
                        }
                    }
                    else
                    {
                        if (request.Order[i].Dir == "asc")
                        {
                            apiOrdersQuery = Queryable.ThenBy((IOrderedQueryable<ApiOrder>)apiOrdersQuery, lambda);
                        }
                        else
                        {
                            apiOrdersQuery = Queryable.ThenByDescending((IOrderedQueryable<ApiOrder>)apiOrdersQuery, lambda);
                        }
                    }
                }
            }

            var apiOrders = await apiOrdersQuery
                .Skip(request.Start)
                .Take(request.Length)
                .ToListAsync();

            joinedApiOrders = apiOrders
                .GroupJoin(
                    stagingStatus,
                    order => order.StagingStatusId,
                    status => status.Id,
                    (order, matchingStatus) => new ApiOrder()
                    {
                        Id = order.Id,
                        BranchId = order.BranchId,
                        GlobalLocationId = order.GlobalLocationId,
                        OrderType = order.OrderType,
                        OrderIsPaid = order.OrderIsPaid,
                        SubTotal = order.SubTotal,
                        DiscountAmount = order.DiscountAmount,
                        GrandTotal = order.GrandTotal,
                        PaymentType = order.PaymentType,
                        OrderSource = order.OrderSource,
                        OrderModel = order.OrderModel,
                        StagingStatusId = order.StagingStatusId,
                        AppId = order.AppId,
                        AppOrderId = order.AppOrderId,
                        AppOrderNumber = order.AppOrderNumber,
                        AppOrderReceiveDatetime = order.AppOrderReceiveDatetime,
                        AppOrderPickupDatetime = order.AppOrderPickupDatetime,
                        PosOrderId = order.PosOrderId,
                        PosOrderNumber = order.PosOrderNumber,
                        StagingStatus = matchingStatus.FirstOrDefault()
                    })
                .ToList();
        }

        return Ok(new DatatableResponse
        {
            Draw = request.Draw,
            RecordsTotal = apiOrdersCount,
            RecordsFiltered = apiOrdersCount,
            Data = joinedApiOrders
        });
    }

    [HttpGet("WorkDayPost/{id}")] //return by date model
    public async Task<IActionResult> WorkDayPost(Guid id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var workdaySales =
          //#1 Start With workday table
          await _context.WorkDays.Where(x => x.Id == id)
          .GroupBy(x => new { x.Id }).Select(x => new
          {
              Id = x.Key.Id
          })

          //#3Order detail table // sales only
          .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Id == id)
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.WorkDay.Id,
              oi.OrderHeader.IsReturn,
          })
          .GroupBy(x => new { x.Id }).Select(x => new
          {
              Id = x.Key.Id,
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
          .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Id == id)
          .GroupBy(x => new { x.OrderHeader.WorkDay.Id }).Select(x => new
          {
              Id = x.Key.Id,
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
          .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Id == id)
             .Select(oi => new
             {
                 oi,
                 oi.OrderHeader.WorkDay.Id,
                 oi.OrderHeader.IsReturn
             })
          .GroupBy(x => new { x.Id }).Select(x => new
          {
              Id = x.Key.Id,
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
              .Select(c => new SalesReportByDateModel()
              {
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
              }).AsNoTracking().FirstOrDefaultAsync();

        var workdayPayments =
        //#1 Start With workday table
        await _context.OrderPayments.Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Id == id)
        .Select(x => new
        {
            Id = x.Payment.Id,
            Name = x.Payment.Name,
            Sname = x.Payment.Sname,
            TotalAmount = x.OrderHeader.IsReturn ? 0 : x.Amount,
            RefundAmount = x.OrderHeader.IsReturn ? x.Amount : 0,
        })
        .GroupBy(x => new { x.Id, x.Name, x.Sname }).Select(x => new SalesReportByPaymentModel
        {
            Id = x.Key.Id,
            Name = x.Key.Name,
            Sname = x.Key.Sname,
            TotalAmount = x.Sum(c => c.TotalAmount).ToString("N2"),
            RefundAmount = x.Sum(c => c.RefundAmount).ToString("N2"),
            NetAmount = (x.Sum(c => c.TotalAmount) - x.Sum(c => c.RefundAmount)).ToString("N2")
        })

       .AsNoTracking().ToListAsync();

        return Ok(new
        {
            sales = workdaySales,
            payments = workdayPayments
        });

    }

    private async Task<List<SalesReportByDateModel>> GetSalesByDateAsync(DateTime from, DateTime to, string branches = "all")
    {

        var _companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;

        POSContext _ReportContext = new POSContext(_companyId, _masterContext, _encMaster.AppSettings);
        _ReportContext.Database.SetCommandTimeout(120);

        await using var transaction = await _ReportContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
           //#1 Start With workday table
           await _ReportContext.WorkDays.Where(x => x.OrderHeaders.Count() > 0 && x.Date >= from.Date && x.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId))) // sales only
           .GroupBy(x => new { x.Date }).Select(x => new
           {
               Date = x.Key.Date
           })

           //#2 Sales Header 
           .GroupJoin(_ReportContext.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId)))
           .GroupBy(x => new { x.WorkDay.Date }).Select(x => new
           {
               Date = x.Key.Date,
               SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
               SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
               CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
               GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
           }), c => c.Date, o => o.Date, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Date = c.c.Date,
               SalesOrdersCount = (o == null) ? null : (decimal?)o.SalesOrdersCount,
               SalesOrdersCountReturn = (o == null) ? null : (decimal?)o.SalesOrdersCountReturn,
               CustomerCount = (o == null) ? null : (decimal?)o.CustomerCount,
               GuestCount = (o == null) ? null : (decimal?)o.GuestCount,
           })


           //#3Order detail table // sales only
           .GroupJoin(_ReportContext.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
           .Select(oi => new
           {
               oi,
               oi.OrderHeader.WorkDay.Date,
               oi.OrderHeader.IsReturn,
           })
           .GroupBy(x => new { x.Date }).Select(x => new
           {
               Date = x.Key.Date,
               ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
               ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
               ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
               ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
               ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
               ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
               ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
               ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
           }), c => c.Date, o => o.Date, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Date = c.c.Date,
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
           .GroupJoin(_ReportContext.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
           .GroupBy(x => new { x.OrderHeader.WorkDay.Date }).Select(x => new
           {
               Date = x.Key.Date,
               ProductsVoidQuantity = x.Sum(z => z.Quantity),
               ProductsVoidAmount = x.Sum(z => z.Total)
           }), c => c.Date, o => o.Date, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Date = c.c.Date,
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
           .GroupJoin(_ReportContext.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
              .Select(oi => new
              {
                  oi,
                  oi.OrderHeader.WorkDay.Date,
                  oi.OrderHeader.IsReturn
              })
           .GroupBy(x => new { x.Date }).Select(x => new
           {
               Date = x.Key.Date,
               FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
               FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
               FeesDiscount = 0,
               FeesDiscountReturn = 0,
               FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
               FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
           }), c => c.Date, o => o.Date, (c, o) => new
           {
               c = c,
               o = o
           })
           .SelectMany(c => c.o.DefaultIfEmpty(), (c, o) => new
           {
               Date = c.c.Date,
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
               .Select(c => new SalesReportByDateModel()
               {
                   Date = c.Date.ToString("yyyy-MM-dd"),
                   OrdersCount = (c.SalesOrdersCount - c.SalesOrdersCountReturn).ToString("N0"),
                   AverageOrder = (c.SalesOrdersCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.SalesOrdersCount, 2)).ToString("N2"),
                   AveragePerGuest = (c.GuestCount == 0 ? 0 : Math.Round(((c.ProductsBeforeDiscount - c.ProductsDiscount + c.FeesBeforeDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn - c.FeesBeforeDiscountReturn)) / c.GuestCount)).ToString("N2"),
                   CustomersCount = c.CustomerCount.ToString("N0"),
                   GuestsCount = c.GuestCount.ToString("N0"),
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
    private async Task<List<SalesReportTopModel>> GetTopSalesByItemAsync(DateTime from, DateTime to, string branches = "all", string itemgroups = "all")
    {
        var _companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;

        POSContext _ReportContext = new POSContext(_companyId, _masterContext, _encMaster.AppSettings);
        _ReportContext.Database.SetCommandTimeout(120);
        await using var transaction = await _ReportContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);
        var result = await _ReportContext.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.Item,
              oi.OrderHeader.IsReturn,
          })
          .GroupBy(x => new { x.Item.Id, x.Item.Name, x.Item.Sname }).Select(x => new
          {
              Id = x.Key.Id,
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
          })
          .Select(c => new SalesReportTopModel()
          {
              Id = c.Id,
              Name = c.Name,
              Sname = c.Sname,
              Amount = (c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn)
          })
          .OrderByDescending(x => x.Amount).Take(5).AsNoTracking().ToListAsync();

        return result;
    }
    private async Task<List<SalesReportTopModel>> GetTopSalesByItemModifierAsync(DateTime from, DateTime to, string branches = "all", string itemgroups = "all")
    {
        var _companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;

        POSContext _ReportContext = new POSContext(_companyId, _masterContext, _encMaster.AppSettings);
        _ReportContext.Database.SetCommandTimeout(120);
        await using var transaction = await _ReportContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result = await _ReportContext.OrderItems.Where(x => x.Modifier && !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.IsReturn,
          })
          .GroupBy(x => new { x.oi.Item.Id, x.oi.Item.Name, x.oi.Item.Sname }).Select(x => new
          {
              Id = x.Key.Id,
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
          })
          .Select(c => new SalesReportTopModel()
          {
              Id = c.Id,
              Name = c.Name,
              Sname = c.Sname,
              Amount = (c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn)
          })
          .OrderByDescending(x => x.Amount).Take(5).AsNoTracking().ToListAsync();

        return result;
    }
    private async Task<List<SalesReportTopModel>> GetTopSalesByItemGroupAsync(DateTime from, DateTime to, string branches = "all", string itemgroups = "all")
    {
        var _companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;

        POSContext _ReportContext = new POSContext(_companyId, _masterContext, _encMaster.AppSettings);
        _ReportContext.Database.SetCommandTimeout(120);
        await using var transaction = await _ReportContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result = await _ReportContext.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.IsReturn,
          })
          .GroupBy(x => new { x.oi.Item.ItemGroup.Id, x.oi.Item.ItemGroup.Name, x.oi.Item.ItemGroup.Sname }).Select(x => new
          {
              Id = x.Key.Id,
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
          })
          .Select(c => new SalesReportTopModel()
          {
              Id = c.Id,
              Name = c.Name,
              Sname = c.Sname,
              Amount = (c.ProductsBeforeDiscount - c.ProductsDiscount - c.ProductsBeforeDiscountReturn + c.ProductsDiscountReturn)
          })
          .OrderByDescending(x => x.Amount).Take(5).AsNoTracking().ToListAsync();

        return result;
    }
    private async Task<List<SalesReportTopModel>> GetTopSalesByPaymentTypeAsync(DateTime from, DateTime to, string branches = "all", string itemgroups = "all")
    {
        var _companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;

        POSContext _ReportContext = new POSContext(_companyId, _masterContext, _encMaster.AppSettings);
        _ReportContext.Database.SetCommandTimeout(120);
        await using var transaction = await _ReportContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
         //#1 Start With workday table
         await _ReportContext.OrderPayments.Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
         .Select(x => new
         {
             Id = x.Payment.Id,
             Name = x.Payment.Name,
             Sname = x.Payment.Sname,
             Amount = x.OrderHeader.IsReturn ? (x.Amount * -1) : x.Amount
         })
         .GroupBy(x => new { x.Id, x.Name, x.Sname }).Select(x => new SalesReportTopModel
         {
             Id = x.Key.Id,
             Name = x.Key.Name,
             Sname = x.Key.Sname,
             Amount = x.Sum(c => c.Amount)
         })

         .OrderByDescending(x => x.Amount).Take(5)

        .AsNoTracking().ToListAsync();

        return result;
    }

}
