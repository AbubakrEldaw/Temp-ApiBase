using System.Diagnostics;
using System.Linq.Expressions;
using APIBase.Helpers;
using APIBase.Models.Enums;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Models.ReportsModels;
using APIBase.Services;
using APIBase.Helpers;
using APIBase.Models.Enums;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Models.ReportsModels;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Company = APIBase.Models.Master.Company;

namespace APIBase.Controllers.Reports;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Roles = Role.Reports)]
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]

public partial class ReportsController : Controller
{
    private readonly POSContext _context;
    private forkpos_masterContext _masterContext;
    private IEncMaster _encMaster;
    public ReportsController(POSContext poscontext, forkpos_masterContext MasterContext, IEncMaster encMaster)
    {
        _masterContext = MasterContext;
        _context = poscontext;
        _context.Database.SetCommandTimeout(120);
        _encMaster = encMaster;
    }
}
