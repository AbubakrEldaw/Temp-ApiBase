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
public class AccountsController : Controller
{
    private readonly POSContext _context;
    private IEncMaster _encMaster;
    private readonly forkpos_masterContext _masterContext;

    public AccountsController(POSContext poscontext, forkpos_masterContext forkpos_MasterContext ,IEncMaster encMaster)
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
            var companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;
            var company = await _masterContext.Companies.FirstOrDefaultAsync(x => x.Id == companyId);
            if(company.AccountId == null)
            {
                company.AccountId = company.Id;
                await _masterContext.SaveChangesAsync();
            }

            var account = await _masterContext.Accounts.Include(x => x.Companies).AsNoTracking().FirstOrDefaultAsync(x => x.Id == company.AccountId);
            return Ok(account);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

}

