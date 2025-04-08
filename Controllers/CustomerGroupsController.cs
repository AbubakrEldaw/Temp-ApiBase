using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBase.Models.POS;
using Microsoft.AspNetCore.Authorization;
using APIBase.Services;
using APIBase.Models.ReportsModels;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class CustomerGroupsController : ControllerBase
{
    private readonly POSContext _context;
    private IEncMaster _encMaster;

    public CustomerGroupsController(POSContext context, IEncMaster encMaster)
    {
        _context = context;
        _encMaster = encMaster;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerGroup>>> GetCustomerGroups()
    {
        try
        {
            var customerGroups = await _context.CustomerGroups.Include(x => x.CustomerCustomerGroups).ThenInclude(x=> x.Customer)
                                                              .AsNoTracking().ToListAsync();
            return Ok(customerGroups);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}
