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
        return await _context.Payments.Include(x => x.PaymentType).Include(x => x.Status).Where(x=> !excludedIds.Contains(x.Id)).ToListAsync();
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
