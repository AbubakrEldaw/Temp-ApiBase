using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class BranchesController : Controller
{
    private readonly POSContext _context;
    private IEncMaster _encMaster;
    private readonly forkpos_masterContext _masterContext;

    public BranchesController(POSContext poscontext, forkpos_masterContext forkpos_MasterContext ,IEncMaster encMaster)
    {
        _context = poscontext;
        _encMaster = encMaster;
        _masterContext = forkpos_MasterContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var branches = await _context.Branches.Include(x => x.VatGroup).AsNoTracking().ToListAsync();
            return Ok(branches);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

 
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        try
        {
            var branch = await _context.Branches.Include(x => x.VatGroup).AsNoTracking().Where(x => x.Id == id).FirstAsync();
            return Ok(branch);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }

    }

}

