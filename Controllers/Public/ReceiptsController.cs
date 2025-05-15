using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBase.Models.POS;
using APIBase.Models.Master;
using APIBase.Services;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using APIBase.PublicAPIModels;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Humanizer;
using APIBase.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Drawing.Printing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;
using Newtonsoft.Json;

namespace APIBase.Controllers.v2;
[Authorize]
[ApiVersion("2.0")]
[ApiController]
[Route("public/v{version:apiVersion}/[controller]")]

public class ReceiptsController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private forkpos_masterContext _MasterContext;
    private readonly IEncMaster _encMaster;

    public ReceiptsController(forkpos_masterContext MasterContext, IEncMaster encMaster, IWebHostEnvironment webHostEnvironment)
    {
        _MasterContext = MasterContext;
        _encMaster = encMaster;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet("GetById")]
    [Tags("Receipts")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [MapToApiVersion("2.0")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, StatusCode = 200, Type = typeof(WorkShift[]))]
    [ProducesResponseType(StatusCodes.Status204NoContent, StatusCode = 204)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BasicError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BasicError))]
    public async Task<ActionResult> GetReceiptsById(string locationId, string receiptId)
    {
        try
        {
            var AppId = User.Claims.FirstOrDefault(x => x.Type.Equals("MarketPlaceAppId", StringComparison.OrdinalIgnoreCase))?.Value;
            var _mpApp = await _MasterContext.MarketPlaceApps.FirstOrDefaultAsync(x => x.Id == AppId);

            if (_mpApp == null) { return BadRequest(new JObject() { { "Error", "App is not activated for this account!" } }); }

            var _companyBranch = await _MasterContext.CompanyBranches.Where(x => x.GlobalBranchId.ToString() == locationId).FirstOrDefaultAsync();
            if (_companyBranch == null)
            {
                return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "Location id is not valid!" });

            }

            var _companyApp = await _MasterContext.CompanyApps.Where(x => x.CompanyId == _companyBranch.CompanyId && x.MarketPlaceAppId == AppId).FirstOrDefaultAsync();
            if (_companyApp == null)
            {
                return Unauthorized(new BasicError() { Error = "Error", ErrorDescription = "App is not activated in this account!" });
            }

            POSContext _posContext = new POSContext(_companyApp.CompanyId, _MasterContext, _encMaster.AppSettings);

            var guid = new Guid(receiptId);
            var receipt = await _posContext.OrderHeaders.Where(x => x.Id == guid).Select(x => new Receipt
            {

                Id = x.Id.ToString(),
                WorkDayDate = x.WorkDay.Date.ToString("yyyy-MM-dd"),
                LocationId = locationId,
                Type = x.IsReturn ? "Return" : "Sales",
                OrderNumber = x.OrderNumber,
                OrderSourceId = x.OrderSource.Id,
                OrderSource = x.OrderSource.Name,
                DiningOptionId = x.DiningOption.Id,
                DiningOption = x.DiningOption.Name,
                TotalDiscount = x.OrderItems.Sum(y => y.Void ? 0 : y.DiscountAmount) + x.HeaderDiscountAmount,
                TotalFees = x.FeesTotal,
                TotalTaxes = x.TotalVat,
                NetTotal = x.Total,
                CreateTime = x.CreateAt.ToUniversalTime().ToString("O"),
                LastModifiedTime = (x.ModifyAt == null ? null : x.ModifyAt.Value.ToUniversalTime().ToString("O")),
                PaidTime = (x.PaidAt == null ? null : x.PaidAt.Value.ToUniversalTime().ToString("O")),
                InvoiceNumber = x.InvoiceNumber,
                ReceiptLines = x.OrderItems.Where(o => !o.Modifier).Select(l => new ReceiptLine
                {
                    Id = l.Id.ToString(),
                    ItemId = l.ItemId,
                    VariantId = l.VariantId,
                    ItemDescription = l.ItemDescription,
                    Quantity = l.Quantity,
                    Price = l.Price,
                    TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                    Total = l.Total,
                    PriceTaxInclusive = l.PriceVatInclusive ?? false,
                    TaxAmount = l.VatAmount,
                    CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                    Void = l.Void,
                    VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                    Modifiers = l.InverseModifierParentNavigation.Select(l => new ModifierLine
                    {
                        Id = l.Id.ToString(),
                        ItemId = l.ItemId,
                        ItemDescription = l.ItemDescription,
                        Quantity = l.Quantity,
                        Price = l.Price,
                        TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                        Total = l.Total,
                        PriceTaxInclusive = l.PriceVatInclusive ?? false,
                        TaxAmount = l.VatAmount,
                        CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                        Void = l.Void,
                        VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                    }).ToList()
                }).ToList(),
                Payments = x.OrderPayments.Select(p => new ReceiptPayment
                {
                    LineIndex = p.LineIndex,
                    PaymentType = p.Payment.Name,
                    Amount = p.Amount
                }).ToList()
            }).AsNoTracking().FirstOrDefaultAsync();

            if (receipt != null)
                return Ok(receipt);

            return NoContent();
        }
        catch (Exception)
        {
            return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "Something went wrong." });
        }
    }


    [HttpGet("GetByDate")]
    [Tags("Receipts")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [MapToApiVersion("2.0")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, StatusCode = 200, Type = typeof(WorkShift[]))]
    [ProducesResponseType(StatusCodes.Status204NoContent, StatusCode = 204)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BasicError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BasicError))]
    public async Task<ActionResult> GetReceiptsByDate(string locationId, DateTime date)
    {
        try
        {
            var AppId = User.Claims.FirstOrDefault(x => x.Type.Equals("MarketPlaceAppId", StringComparison.OrdinalIgnoreCase))?.Value;
            var _mpApp = await _MasterContext.MarketPlaceApps.FirstOrDefaultAsync(x => x.Id == AppId);

            if (_mpApp == null) { return BadRequest(new JObject() { { "Error", "App is not activated for this account!" } }); }

            var _companyBranch = await _MasterContext.CompanyBranches.Where(x => x.GlobalBranchId.ToString() == locationId).FirstOrDefaultAsync();
            if (_companyBranch == null)
            {
                return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "Location id is not valid!" });

            }

            var _companyApp = await _MasterContext.CompanyApps.Where(x => x.CompanyId == _companyBranch.CompanyId && x.MarketPlaceAppId == AppId).FirstOrDefaultAsync();
            if (_companyApp == null)
            {
                return Unauthorized(new BasicError() { Error = "Error", ErrorDescription = "App is not activated in this account!" });
            }

            POSContext _posContext = new POSContext(_companyApp.CompanyId, _MasterContext, _encMaster.AppSettings);

            var receipts = await _posContext.OrderHeaders.Where(x => x.CreateAt.Date == date && x.BranchId == _companyBranch.PosBranchId).OrderBy(x => x.CreateAt).Select(x => new Receipt
            {

                Id = x.Id.ToString(),
                WorkDayDate = x.WorkDay.Date.ToString("yyyy-MM-dd"),
                LocationId = locationId,
                Type = x.IsReturn ? "Return" : "Sales",
                OrderNumber = x.OrderNumber,
                OrderSourceId = x.OrderSource.Id,
                OrderSource = x.OrderSource.Name,
                DiningOptionId = x.DiningOption.Id,
                DiningOption = x.DiningOption.Name,
                TotalDiscount = x.OrderItems.Sum(y => y.Void ? 0 : y.DiscountAmount) + x.HeaderDiscountAmount,
                TotalFees = x.FeesTotal,
                TotalTaxes = x.TotalVat,
                NetTotal = x.Total,
                CreateTime = x.CreateAt.ToUniversalTime().ToString("O"),
                LastModifiedTime = (x.ModifyAt == null ? null : x.ModifyAt.Value.ToUniversalTime().ToString("O")),
                PaidTime = (x.PaidAt == null ? null : x.PaidAt.Value.ToUniversalTime().ToString("O")),
                InvoiceNumber = x.InvoiceNumber,
                ReceiptLines = x.OrderItems.Where(o => !o.Modifier).Select(l => new ReceiptLine
                {
                    Id = l.Id.ToString(),
                    ItemId = l.ItemId,
                    VariantId = l.VariantId,
                    ItemDescription = l.ItemDescription,
                    Quantity = l.Quantity,
                    Price = l.Price,
                    TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                    Total = l.Total,
                    PriceTaxInclusive = l.PriceVatInclusive ?? false,
                    TaxAmount = l.VatAmount,
                    CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                    Void = l.Void,
                    VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                    Modifiers = l.InverseModifierParentNavigation.Select(l => new ModifierLine
                    {
                        Id = l.Id.ToString(),
                        ItemId = l.ItemId,
                        ItemDescription = l.ItemDescription,
                        Quantity = l.Quantity,
                        Price = l.Price,
                        TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                        Total = l.Total,
                        PriceTaxInclusive = l.PriceVatInclusive ?? false,
                        TaxAmount = l.VatAmount,
                        CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                        Void = l.Void,
                        VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                    }).ToList()
                }).ToList(),
                Payments = x.OrderPayments.Select(p => new ReceiptPayment
                {
                    LineIndex = p.LineIndex,
                    PaymentType = p.Payment.Name,
                    Amount = p.Amount
                }).ToList()
            }).AsNoTracking().ToListAsync();
            //WorkShiftResponse r = new WorkShiftResponse() { Workshifts = receipts };
            if (receipts.Any())
                return Ok(receipts);

            return NoContent();
        }
        catch (Exception)
        {
            return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "Something went wrong." });
        }
    }

    [HttpGet("GetByPeriod")]
    [Tags("Receipts")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [MapToApiVersion("2.0")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, StatusCode = 200, Type = typeof(PagedResult<Receipt>))]
    [ProducesResponseType(StatusCodes.Status204NoContent, StatusCode = 204)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BasicError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BasicError))]
    public async Task<ActionResult> GetReceiptsByPeriod(string? locationId = null, DateTime? fromDate = null, DateTime? toDate = null, int currentPage = 0)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var AppId = User.Claims.FirstOrDefault(x => x.Type.Equals("MarketPlaceAppId", StringComparison.OrdinalIgnoreCase))?.Value;

            if (AppId == null)
                return Unauthorized(new JObject() { { "Error", "Unauthorized!" } });

            var _mpApp = await _MasterContext.MarketPlaceApps.FirstOrDefaultAsync(x => x.Id == AppId);

            if (_mpApp == null)
                return BadRequest(new JObject() { { "Error", "App is not activated for this account!" } });

            var _companyBranch = await _MasterContext.CompanyBranches.Where(x => x.GlobalBranchId.ToString() == locationId).FirstOrDefaultAsync();
            if (_companyBranch == null)
            {
                return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "Location id is not valid!" });
            }

            var _companyApp = await _MasterContext.CompanyApps.Where(x => x.CompanyId == _companyBranch.CompanyId && x.MarketPlaceAppId == AppId).FirstOrDefaultAsync();
            if (_companyApp == null)
            {
                return Unauthorized(new BasicError() { Error = "Error", ErrorDescription = "App is not activated in this account!" });
            }

            if (string.IsNullOrEmpty(locationId) || !fromDate.HasValue || !toDate.HasValue || currentPage == 0)
                return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "The query parameters are not complete" });

            // todo: Deserialize into the correct model after the marketplace app is done;
            //var allowedBranches = JsonConvert.DeserializeObject<List<string>>(_companyApp.JsonProp);

            //if (allowedBranches == null || !allowedBranches.Contains(locationId))
            //{
            //    return Unauthorized(new BasicError() { Error = "Error", ErrorDescription = "App is not activated in this account!" });
            //}

            if (fromDate > toDate)
                return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "The beginning date cannot be bigger than the end date" });

            TimeSpan diff = toDate.Value - fromDate.Value;

            //not more 
            //if (Math.Abs(diff.TotalDays) > 31)
            //    return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "The difference between the dates cannot exceed one month" });

            POSContext _posContext = new POSContext(_companyApp.CompanyId, _MasterContext, _encMaster.AppSettings);

            PagedResult<Receipt>? result = await GetReceipts(_posContext, _companyBranch, fromDate.Value, toDate.Value, currentPage);


            stopwatch.Stop();

            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed} ms");

            if (result == null || !result.Items.Any())
            {
                return NoContent();
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest(new BasicError() { Error = "Error", ErrorDescription = "Something went wrong." });
        }
    }

    [HttpGet("GetByWorkshift")]
    [MapToApiVersion("2.0")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, StatusCode = 200, Type = typeof(List<WorkShift>))]
    [ProducesResponseType(StatusCodes.Status204NoContent, StatusCode = 204)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetReceiptsByWorkshift(string locationId, DateTime date)
    {
        try
        {
            var AppId = User.Claims.FirstOrDefault(x => x.Type.Equals("MarketPlaceAppId", StringComparison.OrdinalIgnoreCase))?.Value;
            var _mpApp = await _MasterContext.MarketPlaceApps.FirstOrDefaultAsync(x => x.Id == AppId);

            if (_mpApp == null) { return BadRequest(new JObject() { { "Error", "App is not activated for this account!" } }); }

            var _companyBranch = await _MasterContext.CompanyBranches.Where(x => x.GlobalBranchId.ToString() == locationId).FirstOrDefaultAsync();
            if (_companyBranch == null)
            {
                return BadRequest(new JObject() { { "Error", "Location id is not valid!" } });
            }

            var _companyApp = await _MasterContext.CompanyApps.Where(x => x.CompanyId == _companyBranch.CompanyId && x.MarketPlaceAppId == AppId).FirstOrDefaultAsync();
            if (_companyApp == null)
            {
                return BadRequest(new JObject() { { "Error", "App is not activated in this account!" } });
            }

            POSContext _posContext = new POSContext(_companyApp.CompanyId, _MasterContext, _encMaster.AppSettings);

            var workShifts = await _posContext.WorkDays.Where(x => x.Date == date && x.BranchId == _companyBranch.PosBranchId).OrderBy(x => x.OpenAt).Select(w => new WorkShift
            {
                Id = w.Id.ToString(),
                Date = w.Date.ToString("yyyy-MM-dd"),
                OpenTime = w.OpenAt.ToUniversalTime().ToString("O"),
                CloseTime = (w.CloseAt == null ? null : w.CloseAt.Value.ToUniversalTime().ToString("O")),
                Receipts = w.OrderHeaders.OrderBy(x => x.OrderNumber).Select(x => new Receipt
                {

                    Id = x.Id.ToString(),
                    WorkDayDate = x.WorkDay.Date.ToString("yyyy-MM-dd"),
                    LocationId = locationId,
                    Type = x.IsReturn ? "Return" : "Sales",
                    OrderNumber = x.OrderNumber,
                    OrderSourceId = x.OrderSource.Id,
                    OrderSource = x.OrderSource.Name,
                    DiningOptionId = x.DiningOption.Id,
                    DiningOption = x.DiningOption.Name,
                    TotalDiscount = x.OrderItems.Sum(y => y.Void ? 0 : y.DiscountAmount) + x.HeaderDiscountAmount,
                    TotalFees = x.FeesTotal,
                    TotalTaxes = x.TotalVat,
                    NetTotal = x.Total,
                    CreateTime = x.CreateAt.ToUniversalTime().ToString("O"),
                    LastModifiedTime = (x.ModifyAt == null ? null : x.ModifyAt.Value.ToUniversalTime().ToString("O")),
                    PaidTime = (x.PaidAt == null ? null : x.PaidAt.Value.ToUniversalTime().ToString("O")),
                    InvoiceNumber = x.InvoiceNumber,
                    ReceiptLines = x.OrderItems.Where(o => !o.Modifier).Select(l => new ReceiptLine
                    {
                        Id = l.Id.ToString(),
                        ItemId = l.ItemId,
                        VariantId = l.VariantId,
                        ItemDescription = l.ItemDescription,
                        Quantity = l.Quantity,
                        Price = l.Price,
                        TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                        Total = l.Total,
                        PriceTaxInclusive = l.PriceVatInclusive ?? false,
                        TaxAmount = l.VatAmount,
                        CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                        Void = l.Void,
                        VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                        Modifiers = l.InverseModifierParentNavigation.Select(l => new ModifierLine
                        {
                            Id = l.Id.ToString(),
                            ItemId = l.ItemId,
                            ItemDescription = l.ItemDescription,
                            Quantity = l.Quantity,
                            Price = l.Price,
                            TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                            Total = l.Total,
                            PriceTaxInclusive = l.PriceVatInclusive ?? false,
                            TaxAmount = l.VatAmount,
                            CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                            Void = l.Void,
                            VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                        }).ToList()
                    }).ToList(),
                    Payments = x.OrderPayments.Select(p => new ReceiptPayment
                    {
                        LineIndex = p.LineIndex,
                        PaymentType = p.Payment.PaymentType.Name,
                        Amount = p.Amount
                    }).ToList()

                }).ToList()
            }).AsNoTracking().ToListAsync();

            if (workShifts.Any())
                return Ok(workShifts);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new JObject() { { "Error", "Something went wrong." } });
        }
    }

    private async Task<PagedResult<Receipt>> GetReceipts(POSContext _posContext, CompanyBranch companyBranch, DateTime fromDate, DateTime toDate, int currentPage)
    {
        int receiptsCount = await _posContext.OrderHeaders
            .Where
            (
                x =>
                    x.CreateAt >= fromDate &&
                    x.CreateAt <= toDate &&
                    x.BranchId == companyBranch.PosBranchId
            )
            .AsNoTracking()
            .CountAsync();

        List<Receipt> receipts = [];

        if (receiptsCount > 0)
        {
            receipts = await _posContext.OrderHeaders
            .AsNoTracking()
            .Where
            (
                x =>
                    x.CreateAt >= fromDate &&
                    x.CreateAt <= toDate &&
                    x.BranchId == companyBranch.PosBranchId
            )
            .Skip((currentPage - 1) * 100)
            .Take(100)
            .OrderBy(x => x.CreateAt)
            .Select(x => new Receipt
            {
                Id = x.Id.ToString(),
                WorkDayDate = x.WorkDay.Date.ToString("yyyy-MM-dd"),
                LocationId = companyBranch.GlobalBranchId.ToString(),
                Type = x.IsReturn ? "Return" : "Sales",
                OrderNumber = x.OrderNumber,
                OrderSourceId = x.OrderSource.Id,
                OrderSource = x.OrderSource.Name,
                DiningOptionId = x.DiningOption.Id,
                DiningOption = x.DiningOption.Name,
                TotalDiscount = x.OrderItems.Sum(y => y.Void ? 0 : y.DiscountAmount) + x.HeaderDiscountAmount,
                TotalFees = x.FeesTotal,
                TotalTaxes = x.TotalVat,
                NetTotal = x.Total,
                CreateTime = x.CreateAt.ToUniversalTime().ToString("O"),
                LastModifiedTime = (x.ModifyAt == null ? null : x.ModifyAt.Value.ToUniversalTime().ToString("O")),
                PaidTime = (x.PaidAt == null ? null : x.PaidAt.Value.ToUniversalTime().ToString("O")),
                InvoiceNumber = x.InvoiceNumber,
                ReceiptLines = x.OrderItems.Where(o => !o.Modifier).Select(l => new ReceiptLine
                {
                    Id = l.Id.ToString(),
                    ItemId = l.ItemId,
                    VariantId = l.VariantId,
                    ItemDescription = l.ItemDescription,
                    Quantity = l.Quantity,
                    Price = l.Price,
                    TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                    Total = l.Total,
                    PriceTaxInclusive = l.PriceVatInclusive ?? false,
                    TaxAmount = l.VatAmount,
                    CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                    Void = l.Void,
                    VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                    Modifiers = l.InverseModifierParentNavigation.Select(l => new ModifierLine
                    {
                        Id = l.Id.ToString(),
                        ItemId = l.ItemId,
                        ItemDescription = l.ItemDescription,
                        Quantity = l.Quantity,
                        Price = l.Price,
                        TotalDiscountAmount = l.DiscountAmount + l.HeaderDiscountAmount,
                        Total = l.Total,
                        PriceTaxInclusive = l.PriceVatInclusive ?? false,
                        TaxAmount = l.VatAmount,
                        CreateTime = l.CreateAt.ToUniversalTime().ToString("O"),
                        Void = l.Void,
                        VoidType = l.Void ? (l.VoidType.Id == "vt-nowaste" ? "NotWasted" : "Wasted") : null,
                    }).ToList()
                }).ToList(),
                Payments = x.OrderPayments.Select(p => new ReceiptPayment
                {
                    LineIndex = p.LineIndex,
                    PaymentType = p.Payment.Name,
                    Amount = p.Amount
                }).ToList()
            })
            .AsNoTracking()
            .ToListAsync();
        }

        return new PagedResult<Receipt>()
        {
            Items = receipts,
            CurrentPage = currentPage,
            TotalCount = receiptsCount,
            TotalPages = (int)Math.Ceiling((decimal)receiptsCount / 100)
        };
    }
}
