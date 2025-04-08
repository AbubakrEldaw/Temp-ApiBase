using APIBase.Helpers;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Models.ReportsModels;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        if(companies == null && companyId != null)
        {
            Console.WriteLine("Related Companies not found");
            return BadRequest("Related Companies not found");
        }

        if (companies != null && companyId != null)
        {
            var linkedCompanies = JsonConvert.DeserializeObject<List<Company>>(companies.Value);
            if(!linkedCompanies.Select(x=> x.Id).Contains(companyId))
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

        return Ok(aggregatedOrderItems);

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
