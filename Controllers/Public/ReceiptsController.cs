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

}
