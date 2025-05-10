using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using APIBase.Helpers;
using APIBase.Models.Enums;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Models.ReportsModels;
using APIBase.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Company = APIBase.Models.Master.Company;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Roles = Role.Reports)]
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]

public class ReportsController : Controller
{
    private readonly POSContext _context;
    private forkpos_masterContext _masterContext;
    private IEncMaster _encMaster;
    public ReportsController(POSContext poscontext, forkpos_masterContext MasterContext, IEncMaster encMaster)
    {
        _masterContext = MasterContext;
        _context = poscontext;
        _context.Database.SetCommandTimeout(120);
        _encMaster = encMaster;
    }

    [HttpGet("DashboardSummary")] // return DashboardModel
    public async Task<IActionResult> DashboardSummaryAsync(DateTime from, DateTime to, string branches = "all", string companyId = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var currentCompanyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;
        var companies = User.Claims.Where(x => x.Type == "Companies")?.FirstOrDefault();
        if (companies == null && companyId != null)
        {
            Console.WriteLine("Related Companies not found");
            return BadRequest("Related Companies not found");
        }

        if (companies != null && companyId != null)
        {
            var linkedCompanies = JsonConvert.DeserializeObject<List<Company>>(companies.Value);
            if (!linkedCompanies.Select(x => x.Id).Contains(companyId))
            {
                Console.WriteLine("company not related your current company");
                return BadRequest("company not related your current company");
            }
        }

        var posContext = new POSContext(companyId ?? currentCompanyId, _masterContext, _encMaster.AppSettings);

        #region Dashbaord stats
        //#1 Sales Header
        var r1 = await posContext.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId))) // sales only
            .GroupBy(x => 1).Select(x => new
            {
                SalesOrdersCount = x.Sum(x => !x.IsReturn ? 1 : 0),
                SalesOrdersCountReturn = x.Sum(x => x.IsReturn ? 1 : 0),
                CustomerCount = x.Sum(x => x.CustomerId != null ? 1 : 0),
                GuestCount = x.Sum(x => !x.IsReturn ? (x.GuestCount ?? 1) : 0),
            }).AsNoTracking().FirstOrDefaultAsync();

        //#2 Sales Details
        var r2 = await posContext.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.IsReturn
          })
            .GroupBy(x => 1).Select(x => new
            {
                ProductsBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                ProductsBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total + z.oi.DiscountAmount - z.oi.VatAmount) : (z.oi.Total + z.oi.DiscountAmount))),
                ProductsDiscount = x.Sum(z => z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                ProductsDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : (z.oi.DiscountAmount + z.oi.HeaderDiscountAmount)),
                ProductsTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                ProductsTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount),
                ProductsQuantity = x.Sum(z => z.IsReturn ? 0 : z.oi.Quantity),
                ProductsQuantityReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.Quantity),
            }).AsNoTracking().FirstOrDefaultAsync();

        //#3 Sales Void
        var r3 = await posContext.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
            .GroupBy(x => 1).Select(x => new
            {
                ProductsVoidQuantity = x.Sum(z => z.Quantity),
                ProductsVoidAmount = x.Sum(z => z.Total)
            }).AsNoTracking().FirstOrDefaultAsync();

        //#4 Fees Details
        var r4 = await posContext.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
          .Select(oi => new
          {
              oi,
              oi.OrderHeader.IsReturn
          })
            .GroupBy(x => 1).Select(x => new
            {
                FeesBeforeDiscount = x.Sum(z => z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                FeesBeforeDiscountReturn = x.Sum(z => !z.IsReturn ? 0 : ((z.oi.PriceVatInclusive ?? false) == true ? (z.oi.Total - z.oi.VatAmount) : (z.oi.Total))),
                FeesDiscount = 0,
                FeesDiscountReturn = 0,
                FeesTax = x.Sum(z => z.IsReturn ? 0 : z.oi.VatAmount),
                FeesTaxReturn = x.Sum(z => !z.IsReturn ? 0 : z.oi.VatAmount)
            }).AsNoTracking().FirstOrDefaultAsync();


        var model = new DashboardModel()
        {
            //OrdersCount = r1.SalesOrdersCount - r1.SalesOrdersCountReturn,
            //GrossSales = r2.ProductsBeforeDiscount,
            //RefundAmount = r2.ProductsBeforeDiscountReturn
            OrdersCount = (r1?.SalesOrdersCount ?? 0) - (r1?.SalesOrdersCountReturn ?? 0),
            AverageOrder = (r1?.SalesOrdersCount ?? 0) == 0 ? 0 : Math.Round((((r2?.ProductsBeforeDiscount ?? 0) - (r2?.ProductsDiscount ?? 0) + (r4?.FeesBeforeDiscount ?? 0) - (r2?.ProductsBeforeDiscountReturn ?? 0) + (r2?.ProductsDiscountReturn ?? 0) - (r4?.FeesBeforeDiscountReturn ?? 0))) / (r1?.SalesOrdersCount ?? 0), 2),
            AveragePerGuest = (r1?.GuestCount ?? 0) == 0 ? 0 : Math.Round((((r2?.ProductsBeforeDiscount ?? 0) - (r2?.ProductsDiscount ?? 0) + (r4?.FeesBeforeDiscount ?? 0) - (r2?.ProductsBeforeDiscountReturn ?? 0) + (r2?.ProductsDiscountReturn ?? 0) - (r4?.FeesBeforeDiscountReturn ?? 0))) / r1.GuestCount),
            CustomersCount = (r1?.CustomerCount ?? 0),
            GuestsCount = (r1?.GuestCount ?? 0),
            GrossSales = (r2?.ProductsBeforeDiscount ?? 0) + (r2?.ProductsTax ?? 0) + (r4?.FeesBeforeDiscount ?? 0) + (r4?.FeesTax ?? 0) - (r2?.ProductsBeforeDiscountReturn ?? 0) - (r2?.ProductsTaxReturn ?? 0) - (r4?.FeesBeforeDiscountReturn ?? 0) - (r4?.FeesTaxReturn ?? 0),
            NetSales = (r2?.ProductsBeforeDiscount ?? 0) - (r2?.ProductsDiscount ?? 0) + (r4?.FeesBeforeDiscount ?? 0) - (r2?.ProductsBeforeDiscountReturn ?? 0) + (r2?.ProductsDiscountReturn ?? 0) - (r4?.FeesBeforeDiscountReturn ?? 0),
            NetSalesWithVat = ((r2?.ProductsBeforeDiscount ?? 0) + (r4?.FeesBeforeDiscount ?? 0) - (r2?.ProductsDiscount ?? 0) - (r2?.ProductsBeforeDiscountReturn ?? 0) + (r2?.ProductsDiscountReturn ?? 0) - (r4?.FeesBeforeDiscountReturn ?? 0) - (r2?.ProductsDiscountReturn ?? 0)) + ((r2?.ProductsTax ?? 0) + (r4?.FeesTax ?? 0) - (r2?.ProductsTaxReturn ?? 0) - (r4?.FeesTaxReturn ?? 0)),
            NetQuantity = (r2?.ProductsQuantity ?? 0) - (r2?.ProductsQuantityReturn ?? 0),
            VoidAmount = (r3?.ProductsVoidAmount ?? 0),
            VoidQuantity = (r3?.ProductsVoidQuantity ?? 0),
            DiscountAmount = (r2?.ProductsDiscount ?? 0) + (r4?.FeesDiscount ?? 0) - (r2?.ProductsDiscountReturn ?? 0) - (r4?.FeesDiscountReturn ?? 0),
            VatAmount = ((r2?.ProductsTax ?? 0) + (r4?.FeesTax ?? 0) - (r2?.ProductsTaxReturn ?? 0) - (r4?.FeesTaxReturn ?? 0)),
            RefundAmount = (r2?.ProductsBeforeDiscountReturn ?? 0) + (r4?.FeesBeforeDiscountReturn ?? 0),
            RefundQuantity = (r2?.ProductsQuantityReturn ?? 0)
        };
        #endregion Dashbaord stats

        var salesByDateTask = GetSalesByDateAsync(from, to, branches);
        var topItemsTask = GetTopSalesByItemAsync(from, to, branches);
        var topModifiersTask = GetTopSalesByItemModifierAsync(from, to, branches);
        var topItemGroupsTask = GetTopSalesByItemGroupAsync(from, to, branches);
        var topPaymentTypesTask = GetTopSalesByPaymentTypeAsync(from, to, branches);

        await Task.WhenAll(salesByDateTask, topItemsTask, topModifiersTask, topItemGroupsTask, topPaymentTypesTask);

        model.SalesByDate = salesByDateTask.Result;
        model.TopItems = topItemsTask.Result;
        model.TopModifiers = topModifiersTask.Result;
        model.TopItemGroups = topItemGroupsTask.Result;
        model.TopPaymentTypes = topPaymentTypesTask.Result;

        //      model.SalesByDate = await GetSalesByDateAsync(from, to, branches);
        //model.TopItems = await GetTopSalesByItemAsync(from, to, branches);
        //model.TopModifiers = await GetTopSalesByItemModifierAsync(from, to, branches);
        //model.TopItemGroups = await GetTopSalesByItemGroupAsync(from, to, branches);
        //model.TopPaymentTypes = await GetTopSalesByPaymentTypeAsync(from, to, branches);
        //model.ByItems
        //var x = new { model, r1, r2, r3 ,r4};
        return Ok(model);
    }

    [HttpGet("SalesByItem")]
    public async Task<IActionResult> SalesByItemAsync(DateTime from, DateTime to, string branches = "all", string itemgroups = "all", string ordersources = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        // First, let's filter the OrderItems
        var filteredOrderItems = _context.OrderItems
            .Where(x => x.OrderHeader.OrderStatusId == "os-paid"
                && x.OrderHeader.WorkDay.Date >= from.Date
                && x.OrderHeader.WorkDay.Date <= to.Date);

        // If branches is not "all", add another filter
        if (branches != "all")
        {
            var branchesList = branches.Split(',').ToList();
            filteredOrderItems = filteredOrderItems
                .Where(x => branchesList.Contains(x.OrderHeader.WorkDay.BranchId));
        }

        // If itemgroups is not "all", add another filter
        if (itemgroups != "all")
        {
            var itemgroupsList = itemgroups.Split(',').ToList();
            filteredOrderItems = filteredOrderItems
                .Where(x => itemgroupsList.Contains(x.Item.ItemGroupId));
        }

        // If orderSources is not "all", add another filter
        if (ordersources != "all")
        {
            var ordersourcesList = ordersources.Split(',').ToList();
            filteredOrderItems = filteredOrderItems
                .Where(x => ordersourcesList.Contains(x.OrderHeader.OrderSourceId));
        }

        // Next, let's group and aggregate the order items
        var aggregatedOrderItems = await filteredOrderItems
            .Select(oi => new
            {
                oi,
                oi.OrderHeader.IsReturn
            })
            .GroupBy(x => new { x.oi.Item.Id, x.oi.Item.Name, x.oi.Item.Sname, VariantId = x.oi.Variant.Id, VariantName = x.oi.Variant.Name, VariantSname = x.oi.Variant.Sname })
            .Select(x => new
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
                VoidAmount = (c.ProductsVoidAmount).ToString("N2"),
                VoidQuantity = (c.ProductsVoidQuantity).ToString("N2"),
                DiscountAmount = (c.ProductsDiscount - c.ProductsDiscountReturn).ToString("N2"),
                VatAmount = ((c.ProductsTax - c.ProductsTaxReturn)).ToString("N2"),
                RefundAmount = (c.ProductsBeforeDiscountReturn).ToString("N2"),
                RefundQuantity = (c.ProductsQuantityReturn).ToString("N2")
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(aggregatedOrderItems);

    }

    [HttpGet("SalesByEmployee")]
    public async Task<IActionResult> SalesByEmployeeAsync(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
                //#1 Start With workday table
                await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId)))
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
                .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
                .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
                .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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

        return Ok(result);
    }

    [HttpPost("SalesByCustomerDiscount")]
    public async Task<IActionResult> SalesByCustomerDiscount([FromBody] DatatableAPIRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        try
        {
            var stopwatch = Stopwatch.StartNew(); // Start timing

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
                    c.Phone.Contains(request.Search.Value))
                    &&
                    (request.CustomerGroups == null ||
                    c.CustomerCustomerGroups.Any(cg => request.CustomerGroups.Contains(cg.CustomerGroupId)))
                    )
                .Skip(request.Start)
                .Take(request.Length)
                .AsNoTracking()
                .ToList();

            var customerIds = customers
                .Where(c => c.Id != null)
                .Select(c => c.Id)
                .ToList();

            var orderHeader = _context.OrderHeaders
                .Where(oh => request.Discounts.Contains(oh.DiscountId)
                    && oh.CustomerId != null
                    && customerIds.Contains(oh.CustomerId.Value))
                .GroupBy(oh => oh.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalDiscount = g.Sum(oh => oh.HeaderDiscountAmount),
                    TotalSpent = g.Sum(oh => oh.Total)
                }).ToList();

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
                .ToList();

            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");

            return Ok(new DatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = customersCount,
                RecordsFiltered = customersCount,
                Data = customerReports
            });

            //var customerQuery = _context.Customers
            //    .AsNoTracking();

            //var orderHeadersQuery = _context.OrderHeaders
            //    .AsNoTracking();


            //if (request.CustomerGroups != null && request.CustomerGroups.Count > 0)
            //{
            //    customerQuery = customerQuery.Where(c =>
            //        c.CustomerCustomerGroups.Any(cg => request.CustomerGroups.Contains(cg.CustomerGroupId)));
            //}

            //if (!string.IsNullOrEmpty(request.Search.Value))
            //{
            //    customerQuery = customerQuery.Where(c => c.Name.Contains(request.Search.Value) ||
            //                             c.Phone.Contains(request.Search.Value));
            //}

            ////if (request.From != null && request.To != null)
            ////{
            ////    orderHeadersQuery = orderHeadersQuery
            ////        .Where(oh => oh.CreateAt.Date >= request.From.Value.Date && oh.CreateAt.Date <= request.To.Value.Date);
            ////}

            //if (request.Discounts != null && request.Discounts.Count > 0)
            //{
            //    orderHeadersQuery = orderHeadersQuery
            //        .Where
            //        (
            //           oh => request.Discounts.Contains(oh.DiscountId)
            //           || oh.OrderItems.Any(oi => request.Discounts.Contains(oi.DiscountId))
            //        );
            //}

            //var result = orderHeadersQuery
            //    .GroupBy(oh => new { oh.CustomerId })
            //    .Select(grouped => new
            //    {
            //        grouped.Key.CustomerId,
            //        TotalDiscount = grouped.Sum(oh => oh.HeaderDiscountAmount),
            //        TotalSpend = grouped.Sum(oh => oh.Total),
            //    })
            //    .Join(
            //        customerQuery,
            //        grouped => grouped.CustomerId,
            //        customer => customer.Id,
            //        (grouped, customer) => new CustomerReport
            //        {
            //            Phone = customer.Phone,
            //            Name = customer.Name,
            //            Points = customer.Points,
            //            TotalVisits = customer.OrderHeaders.Count(),
            //            FirstVisit = customer.FirstVisit,
            //            LastVisit = customer.LastVisit,
            //            CreatedByName = customer.CreateByNavigation.Name,
            //            CreatedBySname = customer.CreateByNavigation.Sname,
            //            CreateAt = customer.CreateAt,
            //            TotalSpent = grouped.TotalSpend,
            //            TotalDiscount = grouped.TotalDiscount
            //        }
            //    );

            ////var dtoQuery = query
            ////.Select(customer => new CustomerReport
            ////{
            ////    Phone = customer.Phone,
            ////    Name = customer.Name,
            ////    Points = customer.Points,
            ////    TotalVisits = customer.OrderHeaders.Count(),
            ////    FirstVisit = customer.FirstVisit,
            ////    LastVisit = customer.LastVisit,
            ////    CreatedByName = customer.CreateByNavigation.Name,
            ////    CreatedBySname = customer.CreateByNavigation.Sname,
            ////    CreateAt = customer.CreateAt,
            ////    TotalSpent = customer.OrderHeaders.Sum(oh => oh.Total),
            ////    TotalDiscount = customer.OrderHeaders.Where(oh => oh.DiscountId != null)
            ////                                           .SelectMany(oh => oh.OrderItems)
            ////                                           .Sum(oi => oi.DiscountAmount + oi.HeaderDiscountAmount)
            ////});

            //if (request.Order.Count > 0)
            //{
            //    for (int i = 0; i < request.Order.Count; i++)
            //    {
            //        var columnName = request.Columns[request.Order[i].Column].Data;
            //        var parameter = Expression.Parameter(typeof(CustomerReport), "c");
            //        var property = Expression.Property(parameter, columnName);
            //        var convertedProperty = Expression.Convert(property, typeof(object));
            //        var lambda = Expression.Lambda<Func<CustomerReport, object>>(convertedProperty, parameter);

            //        if (i == 0)
            //        {
            //            if (request.Order[i].Dir == "asc")
            //            {
            //                result = Queryable.OrderBy(result, lambda);
            //            }
            //            else
            //            {
            //                result = Queryable.OrderByDescending(result, lambda);
            //            }
            //        }
            //        //else
            //        //{
            //        //    if (request.Order[i].Dir == "asc")
            //        //    {
            //        //        dtoQuery = Queryable.ThenBy((IOrderedQueryable<CustomerReport>)dtoQuery, lambda);
            //        //    }
            //        //    else
            //        //    {
            //        //        dtoQuery = Queryable.ThenByDescending((IOrderedQueryable<CustomerReport>)dtoQuery, lambda);
            //        //    }
            //        //}
            //    }
            //}

            //var filteredCount = await result.CountAsync();

            //var finalResult = await result
            //    .Skip(request.Start)
            //    .Take(request.Length)
            //    .ToListAsync();

            ////stopwatch.Stop();
            ////Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");

            //return Ok(new DatatableResponse
            //{
            //    Draw = request.Draw,
            //    RecordsTotal = filteredCount,
            //    RecordsFiltered = filteredCount,
            //    Data = finalResult
            //});

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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

    [HttpGet("SalesByWorkDay")]
    public async Task<IActionResult> SalesByWorkDayAsync(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
          //#1 Start With workday table
          await _context.WorkDays.Where(x => /*x.OrderHeaders.Count() > 0 &&*/ x.Date >= from.Date && x.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId))) // sales only
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
          .GroupJoin(_context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId)))
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
          .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
          .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
          .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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

        return Ok(result);
    }

    [HttpGet("SalesByPaymentType")] //return payment model
    public async Task<IActionResult> SalesByPaymentTypeAsync(DateTime from, DateTime to, string branches = "all", string groupBy = "")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);


        if (Enum.TryParse<GroupBy>(groupBy, out GroupBy parsedGroupBy))
        {
            switch (parsedGroupBy)
            {
                case GroupBy.Branch:
                    var X = new ReportWrapper()
                    {
                        Id = "PaymentTypeGroupByBranch",
                        Types = new Dictionary<string, string>
                        {
                            ["id"] = "string",
                            ["name"] = "string",
                            ["sname"] = "string",
                            ["branchId"] = "string",
                            ["totalAmount"] = "num",
                            ["refundAmount"] = "num",
                            ["netAmount"] = "num"
                        },
                        Data = await _context.OrderPayments
                            .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
                            .Select(x => new
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
                            .ToListAsync()
                    };
                    return Ok(X);

                case GroupBy.Date:
                    return Ok(new ReportWrapper()
                    {
                        Id = "PaymentTypeGroupByDate",
                        Types = new Dictionary<string, string>
                        {
                            ["id"] = "string",
                            ["name"] = "string",
                            ["sname"] = "string",
                            ["date"] = "date",
                            ["totalAmount"] = "num",
                            ["refundAmount"] = "num",
                            ["netAmount"] = "num"
                        },
                        Data = await _context.OrderPayments
                                .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
                                .Select(x => new
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
                                .ToListAsync()
                    });

                case GroupBy.BranchAndDate:
                    return Ok(new ReportWrapper()
                    {
                        Id = "PaymentTypeGroupByBranchAndDate",
                        Types = new Dictionary<string, string>
                        {
                            ["id"] = "string",
                            ["name"] = "string",
                            ["sname"] = "string",
                            ["branchId"] = "string",
                            ["date"] = "date",
                            ["totalAmount"] = "num",
                            ["refundAmount"] = "num",
                            ["netAmount"] = "num"
                        },
                        Data = await _context.OrderPayments
                                .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
                                .Select(x => new
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
                                .ToListAsync()
                    });

                default:
                    break;
            }
        }

        return Ok(new ReportWrapper()
        {
            Id = "PaymentType",
            Types = new Dictionary<string, string>
            {
                ["id"] = "string",
                ["name"] = "string",
                ["sname"] = "string",
                ["totalAmount"] = "num",
                ["refundAmount"] = "num",
                ["netAmount"] = "num"
            },
            Data = await _context.OrderPayments
                        .Where(x => x.OrderHeader.VoidBy == null && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
                        .Select(x => new
                        {
                            Id = x.Key.Id,
                            Name = x.Key.Name,
                            Sname = x.Key.Sname,
                            TotalAmount = x.Sum(c => c.TotalAmount).ToString("N2"),
                            RefundAmount = x.Sum(c => c.RefundAmount).ToString("N2"),
                            NetAmount = (x.Sum(c => c.TotalAmount) - x.Sum(c => c.RefundAmount)).ToString("N2")
                        })
                        .OrderBy(x => x.Id)
                        .ToListAsync()
        });
    }

    [HttpGet("SalesByOrderSource")]
    public async Task<IActionResult> SalesByOrderSourceAsync(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result =
            //#1 Start With workday table
            await _context.OrderHeaders.Where(x => x.OrderStatusId == "os-paid" && x.VoidBy == null && x.WorkDay.Date >= from.Date && x.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.BranchId)))
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
            .GroupJoin(_context.OrderItems.Where(x => !x.Void && x.OrderHeader.VoidBy == null && x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
            .GroupJoin(_context.OrderItems.Where(x => x.Void && (x.KotPrinted ?? false == true) && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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
            .GroupJoin(_context.OrderFees.Where(x => x.OrderHeader.OrderStatusId == "os-paid" && x.OrderHeader.WorkDay.Date >= from.Date && x.OrderHeader.WorkDay.Date <= to.Date && (branches == "all" ? true : branches.Contains(x.OrderHeader.WorkDay.BranchId)))
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

        return Ok(result);
    }

    [HttpGet("ApiOrders")]
    public async Task<IActionResult> ApiOrders(DateTime from, DateTime to, string branches = "all")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

        var result = await _context.ApiOrders
            .Where
            (
                x => 
                    x.AppOrderReceiveDatetime >= from.Date && 
                    x.AppOrderReceiveDatetime <= to.Date && 
                    (branches == "all" ? true : branches.Contains(x.BranchId))
            )
            // todo: remove
            .Take(100)
            .ToListAsync();

        return Ok(result);
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
