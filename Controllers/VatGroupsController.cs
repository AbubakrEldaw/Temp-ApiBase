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
public class VatGroupsController : ControllerBase
{
    private readonly POSContext _context;

    public VatGroupsController(POSContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VatGroup>>> GetVatGroup()
    {
        return await _context.VatGroups.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VatGroup>> GetVatGroup(string id)
    {
        var vatGroup = await _context.VatGroups.FindAsync(id);

        if (vatGroup == null)
        {
            return NotFound();
        }

        return vatGroup;
    }

    [HttpPut("Update")]
    public async Task<IActionResult> PutVatGroup(VatGroup model)
    {
        var vatgroruptoupdate = _context.VatGroups.Find(model.Id);

        vatgroruptoupdate.Name = model.Name;
        vatgroruptoupdate.Sname = model.Sname;
        vatgroruptoupdate.Percentage = model.Percentage;
        vatgroruptoupdate.ModifyAt = DateTime.UtcNow;
        _context.Entry(vatgroruptoupdate).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {

        }

        return NoContent();
    }

    [HttpPost("Add")]
    public async Task<ActionResult<VatGroup>> PostVatGroup(VatGroup vatGroup)
    {

        //get number series id from setting table
        var ns_id = _context.NumberSeriesSettings.Where(x => x.Id == "vat_group").First().NumberSeriesId;

        //get new id for the saved operation.
        vatGroup.Id = Helpers.NumberSeriesHelper.get_new_id_for_insert(ns_id, _context, null, null);

        vatGroup.CreateAt = DateTime.UtcNow;
        _context.VatGroups.Add(vatGroup);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (VatGroupExists(vatGroup.Id))
            {
                return Conflict();
            }
            else
            {
                throw;
            }
        }

        return CreatedAtAction("GetVatGroup", new { id = vatGroup.Id }, vatGroup);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<VatGroup>> DeleteVatGroup(string id)
    {
        var vatGroup = await _context.VatGroups.FindAsync(id);
        if (vatGroup == null)
        {
            return NotFound();
        }

        _context.VatGroups.Remove(vatGroup);
        await _context.SaveChangesAsync();

        return vatGroup;
    }

    private bool VatGroupExists(string id)
    {
        return _context.VatGroups.Any(e => e.Id == id);
    }
}
