using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBase.Models.POS;
using System.Security.Cryptography.X509Certificates;
using APIBase.Services;
using APIBase.Utils.Encryption;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class ItemGroupsController : ControllerBase
{
    private readonly POSContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IEncMaster _encMaster;
    public ItemGroupsController(POSContext context, IEncMaster encMaster, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _encMaster = encMaster;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemGroup>>> GetItemGroup()
    {
        return await _context.ItemGroups.Include(x => x.Status).Include(x => x.Items).OrderBy(x => x.OrderIndex).AsNoTracking().ToListAsync();
    }

    [HttpGet("GetItemsGrouped")]
    public async Task<ActionResult<IEnumerable<ItemGroup>>> GetItemsGrouped()
    {
        return await _context.ItemGroups.OrderBy(x => x.OrderIndex).Include(x => x.Items.Where(x=> x.StatusId == "st-active")).ThenInclude(x=> x.InverseVariantParent.Where(x=> x.StatusId == "st-active")).AsNoTracking().ToListAsync();
    }

}
