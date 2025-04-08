using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBase.Models.POS;
using Microsoft.AspNetCore.Authorization;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class DiscountsController : ControllerBase
{
    private readonly POSContext _context;

    public DiscountsController(POSContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Discount>>> GetDiscount()
    {
        return await _context.Discounts.Include(x => x.DiscountType).Include(x => x.Status).OrderBy(x => x.Priority).AsNoTracking().ToListAsync();
    }

}
