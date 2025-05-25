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
public class VoidTypesController : ControllerBase
{
    private readonly POSContext _context;

    public VoidTypesController(POSContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VoidType>>> GetVoidReason()
    {
        return await _context.VoidTypes.ToListAsync();
    }
}
