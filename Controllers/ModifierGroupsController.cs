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
public class ModifierGroupsController : ControllerBase
{
    private readonly POSContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IEncMaster _encMaster;
    public ModifierGroupsController(POSContext context, IEncMaster encMaster, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _encMaster = encMaster;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModifierGroup>>> GetModifierGroup()
    {
        return await _context.ModifierGroups.Include(x => x.ModifierGroupItems.OrderBy(x => x.OrderIndex)).ThenInclude(x => x.Item).OrderBy(x => x.OrderIndex).AsNoTracking().ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ModifierGroup>> GetModifierGroup(string id)
    {
        var ModifierGroup = await _context.ModifierGroups.Include(x => x.ItemModifierGroups).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        if (ModifierGroup == null)
            return NotFound();

        return ModifierGroup;
    }

    [HttpPut("Update")]
    public async Task<IActionResult> PutModifierGroup(ModifierGroup model)
    {
        var modifierGrouptoupdate = await _context.ModifierGroups.Include(x => x.ItemModifierGroups).FirstOrDefaultAsync(x => x.Id == model.Id);

        modifierGrouptoupdate.Name = model.Name;
        modifierGrouptoupdate.Sname = model.Sname;
        modifierGrouptoupdate.Min = model.Min;
        modifierGrouptoupdate.Max = model.Max;
        modifierGrouptoupdate.Free = model.Free;
        modifierGrouptoupdate.Multiple = model.Multiple;


        //Modifier Groups
        var DBItemModifierGroups = new List<ItemModifierGroup>(modifierGrouptoupdate.ItemModifierGroups);
        if (model.ItemModifierGroups == null)
        {
            model.ItemModifierGroups = new List<ItemModifierGroup>();
        }

        //Add if not in db
        foreach (var item in model.ItemModifierGroups)
        {
            if (!modifierGrouptoupdate.ItemModifierGroups.Any(x => x.ItemId == item.ItemId && x.ModifierGroupId == item.ModifierGroupId))
            {
                var md = await _context.ModifierGroups.FirstOrDefaultAsync(x => x.Id == item.ModifierGroupId);
                if (md != null)
                {
                    modifierGrouptoupdate.ItemModifierGroups.Add(new ItemModifierGroup() { ItemId = item.ItemId, ModifierGroupId = md.Id, Min = md.Min, Max = md.Max, Free = 0, Multiple = md.Multiple });
                }
            }
        }

        //remove if in db and not in the list
        foreach (var item in DBItemModifierGroups)
        {
            if (!model.ItemModifierGroups.Any(x => x.ItemId == item.ItemId && x.ModifierGroupId == item.ModifierGroupId))
            {
                modifierGrouptoupdate.ItemModifierGroups.Remove(item);
            }
        }

        _context.Entry(modifierGrouptoupdate).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {

            throw e;

        }

        return Ok(modifierGrouptoupdate);
    }

    [HttpPost("Add")]
    public async Task<ActionResult<ModifierGroup>> PostModifierGroup(ModifierGroup model)
    {
        //get number series id from setting table
        var ns_id = _context.NumberSeriesSettings.Where(x => x.Id == "modifier_group").First().NumberSeriesId;

        //get new id for the saved operation.
        model.Id = Helpers.NumberSeriesHelper.get_new_id_for_insert(ns_id, _context, null, null);

        _context.ModifierGroups.Add(model);
        _context.SaveChanges();
        return CreatedAtAction("GetModifierGroup", new { id = model.Id }, model);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ModifierGroup>> DeleteModifierGroup(string id)
    {
        var ModifierGroup = await _context.ModifierGroups.Include(x => x.ModifierGroupItems).Where(x => x.Id == id).FirstOrDefaultAsync();
        if (ModifierGroup == null)
        {
            return NotFound();
        }

        _context.ModifierGroupItems.RemoveRange(ModifierGroup.ModifierGroupItems);
        _context.ModifierGroups.Remove(ModifierGroup);
        _context.SaveChanges();

        return ModifierGroup;
    }

    private bool ModifierGroupExists(string id)
    {
        return _context.ModifierGroups.Any(e => e.Id == id);
    }

    [HttpPost("AddItems")]
    public async Task<ActionResult<List<ModifierGroup>>> PostMenuGroupItems(List<ModifierGroupItem> model)
    {
        try
        {
            if (model.Count == 0)
            {
                return BadRequest();
            }

            var lastIndex = await _context.ModifierGroupItems.Where(x => x.ModifierGroupId == model.First().ModifierGroupId).OrderByDescending(x => x.OrderIndex).Select(x => x.OrderIndex).FirstOrDefaultAsync();

            foreach (var item in model)
            {
                item.OrderIndex = lastIndex++;
            }

            _context.ModifierGroupItems.AddRange(model);
            await _context.SaveChangesAsync();

            var ids = model.Select(x => x.ItemId).ToList();

            var mgis = await _context.ModifierGroupItems.Include(x => x.Item).Where(x => x.ModifierGroupId == model.First().ModifierGroupId && ids.Contains(x.ItemId)).ToListAsync();
            return Ok(mgis);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("RemoveItem")]
    public async Task<ActionResult<ModifierGroup>> PostMenuGroupRemoveItem(ModifierGroupItem model)
    {

        try
        {
            var itemtoremove = await _context.ModifierGroupItems.Where(x => x.ModifierGroupId == model.ModifierGroupId && x.ItemId == model.ItemId).FirstOrDefaultAsync();
            if (itemtoremove != null)
            {
                _context.ModifierGroupItems.Remove(itemtoremove);
                await _context.SaveChangesAsync();
            }

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }


        return Ok();
    }

    [HttpPut("Reorder")]
    public async Task<IActionResult> Reorder(ModifierGroup model)
    {
        var modifierGroups = await _context.ModifierGroups.ToListAsync();

        // Check if initialization is needed
        if (modifierGroups.All(mg => mg.OrderIndex == 0))
        {
            int index = 0;
            modifierGroups.OrderBy(mg => mg.Id).ToList().ForEach(mg => mg.OrderIndex = index++);
            await _context.SaveChangesAsync();
        }

        var modifierGroup = modifierGroups.Find(x => x.Id == model.Id);

        var minIndex = Math.Min(modifierGroup.OrderIndex, model.OrderIndex);
        var maxIndex = Math.Max(modifierGroup.OrderIndex, model.OrderIndex);

        var mgListToUpdate = modifierGroups
            .Where(x => x.OrderIndex >= minIndex && x.OrderIndex <= maxIndex)
            .ToList();

        var change = modifierGroup.OrderIndex > model.OrderIndex ? 1 : -1;

        foreach (var mg in mgListToUpdate)
        {
            if (mg.Id == model.Id)
            {
                mg.OrderIndex = model.OrderIndex;
            }
            else
            {
                mg.OrderIndex += change;
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (DbUpdateConcurrencyException)
        {

        }

        return Ok();
    }

    [HttpPut("ReorderModifierGroupItem")]
    public async Task<IActionResult> ReorderModifierGroupItem(ModifierGroupItem model)
    {
        var modifierGroupItemsList = await _context.ModifierGroupItems.Where(x => x.ModifierGroupId == model.ModifierGroupId).ToListAsync();

        var mgi = modifierGroupItemsList.Find(x => x.ItemId == model.ItemId);

        if (mgi.ModifierGroupId == model.ModifierGroupId)
        {
            var minIndex = mgi.OrderIndex > model.OrderIndex ? model.OrderIndex : mgi.OrderIndex;
            var maxIndex = mgi.OrderIndex >= model.OrderIndex ? mgi.OrderIndex : model.OrderIndex;

            var mgiListToUpdate = modifierGroupItemsList.Where(x => x.OrderIndex >= minIndex && x.OrderIndex <= maxIndex).ToList();
            var change = 0;
            if (mgi.OrderIndex > model.OrderIndex)
            {
                change = +1;
            }
            else
            {
                change = -1;
            }
            foreach (var item in mgiListToUpdate)
            {
                if (item.ItemId == model.ItemId)
                {
                    item.OrderIndex = model.OrderIndex;
                }
                else
                {
                    item.OrderIndex = item.OrderIndex + change;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

            }
        }

        return Ok();
    }

}
