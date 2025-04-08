using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Services;
using APIBase.Utils.Encryption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class CompanyController : ControllerBase
{
    private readonly POSContext _context;
    private IEncMaster _encMaster;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly forkpos_masterContext _MasterContext;

    public CompanyController(POSContext context, IWebHostEnvironment webHostEnvironment, IEncMaster encMaster, forkpos_masterContext MasterContext)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _encMaster = encMaster;
        _MasterContext = MasterContext;
    }

    [HttpGet]
    public async Task<ActionResult<Models.POS.Company>> GetCompany()
    {
        return await _context.Companies.FirstAsync();
    }

    [HttpPut("GetPlan")]
    public async Task<ActionResult<Plan>> GetCompanyPlan()
    {
        var _companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;
        var _plan = await _MasterContext.CompanyPlans.Include(x => x.Plan).ThenInclude(x => x.PlanFeatures).FirstOrDefaultAsync(x => x.CompanyId == _companyId);
        if (_plan != null)
        {
            return Ok(_plan);
        }
        else
        {
            return BadRequest("No plan");
        }
    }
}
