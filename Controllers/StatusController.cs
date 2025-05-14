using System.Collections.Generic;
using System.Threading.Tasks;
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
public class StatusController : ControllerBase
{
    private readonly POSContext _context;

    public StatusController(POSContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Status>>> GetStatusType()
    {
        return await _context.Statuses.ToListAsync();
    }
}

