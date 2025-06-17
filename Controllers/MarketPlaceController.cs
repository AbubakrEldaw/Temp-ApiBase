using APIBase.Helpers;
using APIBase.Models.Master;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Roles = Role.MarketPlace)]
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class MarketPlaceController : Controller
{
    private forkpos_masterContext _MasterContext;
    private IEncMaster _encMaster;
    public MarketPlaceController(IEncMaster encMaster, forkpos_masterContext MasterContext)
    {
        _encMaster = encMaster;
        _MasterContext = MasterContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var _companyId = User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault().Value;

            //       var _plan = await _masterContext.CompanyPlans.Where(x => x.CompanyId == _companyId).Select(x => x.Plan).AsNoTracking().FirstOrDefaultAsync();
            //var _availablePlanAppsIds = await _masterContext.MarketPlaceAppPlanAvailabilities.Where(x => x.PlanId == _plan.Id).Select(x => x.MarketPlaceAppId).AsNoTracking().ToListAsync();
            //var _availablePlanAppsIds = await _masterContext.MarketPlaceAppPlanAvailabilities.Select(x => x.MarketPlaceAppId).AsNoTracking().ToListAsync();
            var _blackListedAppsIds = await _MasterContext.MarketPlaceCompanyBlacklists.Where(x => x.CompanyId == _companyId).Select(x => x.MarketPlaceAppId).AsNoTracking().ToListAsync();
            // _availablePlanAppsIds = _availablePlanAppsIds.Except(_blackListedAppsIds).ToList();

            var extraApps = await _MasterContext.CompanyApps.Where(x => x.CompanyId == _companyId).Select(x => x.MarketPlaceAppId).ToListAsync();
            _blackListedAppsIds = _blackListedAppsIds.Except(extraApps).ToList();

            var MPModel = await _MasterContext.MarketPlaceCategories.Include(x => x.MarketPlaceApps.Where(x => !_blackListedAppsIds.Contains(x.Id) && x.Status == "a")).ThenInclude(x => x.CompanyApps.Where(x => x.CompanyId == _companyId))
                                                                    .Include(x => x.MarketPlaceApps.Where(x => !_blackListedAppsIds.Contains(x.Id) && x.Status == "a")).ThenInclude(x => x.Plans)
                                                                    .AsNoTracking().ToListAsync();

            var companyapps = MPModel.SelectMany(x => x.MarketPlaceApps).SelectMany(x => x.CompanyApps);

            foreach (var item in companyapps)
            {
                item.JsonProp = null;
            }

            return Ok(MPModel);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("GetApp/{id}")]
    public async Task<ActionResult<MarketPlaceApp>> GetMarketPlaceApp(string id)
    {
        var marketPlaceApp = await _MasterContext.MarketPlaceApps.Where(x => x.Id == id).AsNoTracking().FirstOrDefaultAsync();

        if (marketPlaceApp == null)
        {
            return NotFound();
        }

        return Ok(marketPlaceApp);
    }

}
