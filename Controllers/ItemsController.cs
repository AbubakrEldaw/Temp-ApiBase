using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBase.Models.POS;
using Microsoft.AspNetCore.Authorization;
using APIBase.Helpers;

namespace APIBase.Controllers;
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class ItemsController : ControllerBase
{

    private readonly POSContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ItemsController(POSContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetItem()
    {
        return await _context.Items.Where(x => x.VariantParentId == null && x.StatusId != "st-deleted").Include(x => x.InverseVariantParent.Where(x => x.StatusId != "st-deleted")).Include(x => x.ItemDivision).Include(x => x.ItemCategory).Include(x => x.ItemGroup).Include(x => x.VatGroup).Include(x => x.Status).Include(x => x.ItemModifierItems).Include(x => x.ItemNotInBranches).Include(x => x.ItemBranchPrices).Include(x => x.KitchenPrintGroupItems).AsNoTracking().ToListAsync();
    }

    [HttpGet("GetDeletedItems")]
    public async Task<ActionResult<IEnumerable<Item>>> GetDeletedItems()
    {
        var items = await _context.Items.Where(x => x.VariantParentId == null).Include(x => x.InverseVariantParent).AsNoTracking().ToListAsync();
        var deletedItems = new List<Item>();

        foreach (var item in items)
        {
            if (item.StatusId == "st-deleted")
            {
                deletedItems.Add(item);
            }
            else if (item.HasVariants)
            {
                item.InverseVariantParent = item.InverseVariantParent.Where(x => x.StatusId == "st-deleted").ToList();

                if (item.InverseVariantParent.Any())
                {
                    deletedItems.Add(item);
                }
            }
        }

        return deletedItems;
    }

  
    [HttpGet("{id}")]
    public async Task<ActionResult<Item>> GetItem(string id)
    {
        var item = await _context.Items.Include(x => x.InverseVariantParent).ThenInclude(x => x.ItemCategory).Include(x => x.ItemModifierGroups).ThenInclude(x => x.ModifierGroup).Include(x => x.ItemDivision).Include(x => x.ItemCategory).Include(x => x.ItemGroup).Include(x => x.VatGroup).Include(x => x.Status).Include(x => x.ItemModifierItems).Include(x => x.ItemNotInBranches).Include(x => x.ItemBranchPrices).Include(x => x.KitchenPrintGroupItems).AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        return item;
    }


 
  

  
    [HttpGet("Modifiers")]
    public async Task<ActionResult<Item>> GetModifiers()
    {
        var items = await _context.Items.Where(x => x.Modifier && x.StatusId == "st-active").AsNoTracking().ToListAsync();

        return Ok(items);
    }

    [Authorize(Roles = "Console, prm-items")]
    [HttpPut("UpdateVariant")]
    public async Task<IActionResult> PutVariant(Item model)
    {
        var itemtoupdate = await _context.Items.Include(x => x.ItemGroup).Include(x => x.ItemNotInBranches).Include(x => x.ItemBranchPrices).Include(x => x.VatGroup).FirstOrDefaultAsync(i => i.Id == model.Id);


        itemtoupdate.Name = model.Name;
        itemtoupdate.Sname = model.Sname;
        itemtoupdate.Price = model.Price;
        itemtoupdate.StatusId = model.StatusId;
        itemtoupdate.ModifyBy = model.ModifyBy;

        itemtoupdate.Description = model.Description ?? "";
        itemtoupdate.Sdescription = model.Sdescription ?? "";

        itemtoupdate.ImagePath = _context.Companies.First().Id + "/Items";

        #region notinbranch & branchprices
        //Add variant custom prices

        var DBItemNotInBrancheses = new List<ItemNotInBranch>(itemtoupdate.ItemNotInBranches);

        if (model.ItemNotInBranches == null)
        {
            model.ItemNotInBranches = new List<ItemNotInBranch>();
        }

        //Add if not in db
        foreach (var item in model.ItemNotInBranches)
        {
            if (!DBItemNotInBrancheses.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId))
            {
                itemtoupdate.ItemNotInBranches.Add(item);
            }
        }
        //remove if in db and not in the list
        foreach (var item in DBItemNotInBrancheses)
        {
            if (!model.ItemNotInBranches.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId))
            {
                itemtoupdate.ItemNotInBranches.Remove(item);
            }
        }

        var DBItemBranchPrices = new List<ItemBranchPrice>(itemtoupdate.ItemBranchPrices);

        if (model.ItemBranchPrices == null)
        {
            model.ItemBranchPrices = new List<ItemBranchPrice>();
        }

        //Add if not in db
        foreach (var item in model.ItemBranchPrices)
        {
            if (!DBItemBranchPrices.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId && x.Price == item.Price))
            {
                itemtoupdate.ItemBranchPrices.Add(item);
            }
        }
        //remove if in db and not in the list
        foreach (var item in DBItemBranchPrices)
        {
            if (!model.ItemBranchPrices.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId && x.Price == item.Price))
            {
                itemtoupdate.ItemBranchPrices.Remove(item);
            }
        }
        #endregion notinbranch & branchprices

        try
        {
            if (model.ImagePath == null)
            {
                itemtoupdate.ImagePath = _context.Companies.First().Id + "/Items";
            }

            if (model.ImageName == null)
            {
                if (itemtoupdate.ImageName != null)
                {
                    RemoveImage(itemtoupdate.ImagePath, itemtoupdate.ImageName);
                    itemtoupdate.ImageName = null;
                }
            }
            else if (model.ImageName != "")
            {
                RemoveImage(itemtoupdate.ImagePath, itemtoupdate.ImageName);
                itemtoupdate.ImageName = CreateImage(itemtoupdate.ImagePath, itemtoupdate.Id, model.ImageName);

            }
            _context.Entry(itemtoupdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, ErrorHelper.UnknownError);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, ErrorHelper.UnknownError);
        }

        return Ok(await _context.Items.Include(x => x.ItemGroup).Where(x => x.Id == model.Id).AsNoTracking().FirstOrDefaultAsync());

    }

    [Authorize(Roles = "Console, prm-items")]
    [HttpPost("AddVariant")]
    public async Task<IActionResult> PostVariant(Item model)
    {
        var itemtoupdate = await _context.Items.Include(x => x.ItemGroup).Include(x => x.VatGroup).Include(x => x.ItemModifierGroups).FirstOrDefaultAsync(i => i.Id == model.VariantParentId);


        var ns_id = _context.NumberSeriesSettings.Where(x => x.Id == "item").First().NumberSeriesId;

        Item newvariant = new Item();

        //get new id
        newvariant.Id = Helpers.NumberSeriesHelper.get_new_id_for_insert(ns_id, _context, null, null);

        if (newvariant.Id == null)
        {
            return Conflict();
        }

        newvariant.Name = model.Name;
        newvariant.Sname = model.Sname;

        newvariant.ItemDivisionId = null; // to enable
        newvariant.ItemCategoryId = null; // to enable + cascading to the division

        newvariant.ItemGroupId = itemtoupdate.ItemGroupId;
        newvariant.VatGroupId = itemtoupdate.VatGroupId;

        newvariant.Barcode = null; // to enable

        newvariant.Recipe = false;// to enable
        newvariant.UseProduction = false;// to enable

        newvariant.Modifier = false;
        newvariant.HasVariants = false;

        newvariant.Sale = true; // to enable
        newvariant.Price = model.Price;
        newvariant.Cost = 0;

        newvariant.Purchase = false;              // to enable
        newvariant.PurchaseUom = null;            // to enable
        newvariant.RecipeUom = null;              // to enable
        newvariant.PurchaseToRecipeUom = null;    // to enable
        newvariant.ReorderPoint = null;           // to enable
        newvariant.MinQty = null;                 // to enable
        newvariant.MaxQty = null;                 // to enable

        newvariant.StatusId = model.StatusId;
        newvariant.CreateBy = model.CreateBy;

        newvariant.Description = model.Description;
        newvariant.Sdescription = model.Sdescription;

        newvariant.ImagePath = _context.Companies.First().Id + "/Items";

        //Add variant custom prices
        foreach (var item in model.ItemBranchPrices)
        {
            newvariant.ItemBranchPrices.Add(item);
        }

        //Add parent modifier groups to the new varaint 
        if (itemtoupdate.ItemModifierGroups.Any())
        {
            foreach (var modifierGroup in itemtoupdate.ItemModifierGroups)
            {
                newvariant.ItemModifierGroups.Add(new ItemModifierGroup()
                {
                    ItemId = newvariant.Id,
                    ModifierGroupId = modifierGroup.ModifierGroupId,
                    Min = modifierGroup.Min,
                    Max = modifierGroup.Max,
                    Free = modifierGroup.Free,
                    Multiple = modifierGroup.Multiple
                });
            }
        }

        itemtoupdate.InverseVariantParent.Add(newvariant);

        try
        {
            await _context.SaveChangesAsync();

            if (model.ImageName != null)
            {
                newvariant.ImageName = CreateImage(newvariant.ImagePath, newvariant.Id, model.ImageName);


                _context.Entry(newvariant).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            _context.Entry(itemtoupdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return Ok(await _context.Items.Include(x => x.ItemGroup).Where(x => x.Id == newvariant.Id).AsNoTracking().FirstOrDefaultAsync());

    }

    [Authorize(Roles = "Console, prm-items")]
    [HttpPut("Update")]
    public async Task<IActionResult> PutItem(Item model)
    {

        var itemtoupdate = await _context.Items.Include(x => x.ItemModifierItems).Include(x => x.ItemModifierGroups).Include(x => x.ItemNotInBranches).Include(x => x.ItemBranchPrices).Include(x => x.InverseVariantParent).ThenInclude(x => x.ItemModifierGroups).FirstOrDefaultAsync(i => i.Id == model.Id);

        var oldPrice = itemtoupdate.Price;
        itemtoupdate.RowVersion = model.RowVersion;
        itemtoupdate.Name = model.Name;
        itemtoupdate.Sname = model.Sname;

        //itemtoupdate.ItemDivisionId = null; // to enable
        //itemtoupdate.ItemCategoryId = null; // to enable + cascading to the division

        itemtoupdate.ItemGroupId = model.ItemGroupId;
        itemtoupdate.VatGroupId = model.VatGroupId;

        itemtoupdate.Barcode = model.Barcode;

        itemtoupdate.Description = model.Description ?? "";
        itemtoupdate.Sdescription = model.Sdescription ?? "";

        //itemtoupdate.Recipe = false;// to enable
        //itemtoupdate.UseProduction = false;// to enable

        //itemtoupdate.Modifier = model.Modifier;

        if (!itemtoupdate.Modifier)
        {
            var DBItemModifiers = new List<ItemModifier>(itemtoupdate.ItemModifierItems);

            if (model.ItemModifierItems == null)
            {
                model.ItemModifierItems = new List<ItemModifier>();
            }

            //Add if not in db
            foreach (var item in model.ItemModifierItems)
            {
                if (!itemtoupdate.ItemModifierItems.Any(x => x.ItemId == item.ItemId && x.ModifierItemId == item.ModifierItemId))
                {
                    itemtoupdate.ItemModifierItems.Add(item);
                }
            }
            //remove if in db and not in the list
            foreach (var item in DBItemModifiers)
            {
                if (!model.ItemModifierItems.Any(x => x.ItemId == item.ItemId && x.ModifierItemId == item.ModifierItemId))
                {
                    itemtoupdate.ItemModifierItems.Remove(item);
                }
            }

            //Modifier Groups
            var DBItemModifierGroups = new List<ItemModifierGroup>(itemtoupdate.ItemModifierGroups);
            if (model.ItemModifierGroups == null)
            {
                model.ItemModifierGroups = new List<ItemModifierGroup>();
            }

            //Add if not in db
            foreach (var item in model.ItemModifierGroups)
            {
                if (!itemtoupdate.ItemModifierGroups.Any(x => x.ItemId == item.ItemId && x.ModifierGroupId == item.ModifierGroupId))
                {
                    var md = await _context.ModifierGroups.FirstOrDefaultAsync(x => x.Id == item.ModifierGroupId);
                    if (md != null)
                    {
                        itemtoupdate.ItemModifierGroups.Add(new ItemModifierGroup() { ModifierGroupId = md.Id, Min = md.Min, Max = md.Max, Free = 0, Multiple = md.Multiple });

                        if (itemtoupdate.HasVariants)
                        {
                            foreach (var variant in itemtoupdate.InverseVariantParent)
                            {
                                variant.ItemModifierGroups.Add(new ItemModifierGroup() { ModifierGroupId = md.Id, Min = md.Min, Max = md.Max, Free = 0, Multiple = md.Multiple });
                            }
                        }
                    }
                }
            }

            //remove if in db and not in the list
            foreach (var item in DBItemModifierGroups)
            {
                if (!model.ItemModifierGroups.Any(x => x.ItemId == item.ItemId && x.ModifierGroupId == item.ModifierGroupId))
                {
                    itemtoupdate.ItemModifierGroups.Remove(item);

                    if (itemtoupdate.HasVariants)
                    {
                        foreach (var variant in itemtoupdate.InverseVariantParent)
                        {
                            var variantMg = variant.ItemModifierGroups.Where(x => x.ModifierGroupId == item.ModifierGroupId).FirstOrDefault();

                            if (variantMg != null)
                                variant.ItemModifierGroups.Remove(variantMg);
                        }
                    }
                }
            }
        }
        else
        {
            itemtoupdate.ItemModifierItems.Clear();
            if (itemtoupdate.HasVariants)
            {
                foreach (var variant in itemtoupdate.InverseVariantParent)
                {
                    variant.ItemModifierItems.Clear();
                }
            }
        }

        if (itemtoupdate.HasVariants)
        {
            foreach (var var in model.InverseVariantParent)
            {
                var v_ns_id = _context.NumberSeriesSettings.Where(x => x.Id == "item").First().NumberSeriesId;

                Item newvariant = new Item();

                //get new id
                newvariant.Id = Helpers.NumberSeriesHelper.get_new_id_for_insert(v_ns_id, _context, null, null);

                if (newvariant.Id == null)
                {
                    return Conflict();
                }

                newvariant.Name = var.Name;
                newvariant.Sname = var.Sname;

                newvariant.ItemDivisionId = null; // to enable
                newvariant.ItemCategoryId = null; // to enable + cascading to the division

                newvariant.ItemGroupId = model.ItemGroupId;
                newvariant.VatGroupId = model.VatGroupId;

                newvariant.Barcode = null; // to enable

                newvariant.Recipe = false;// to enable
                newvariant.UseProduction = false;// to enable

                newvariant.Modifier = false;
                newvariant.HasVariants = false;

                newvariant.Sale = true; // to enable
                newvariant.Price = model.Price;
                newvariant.Cost = 0;

                newvariant.Purchase = false;              // to enable
                newvariant.PurchaseUom = null;            // to enable
                newvariant.RecipeUom = null;              // to enable
                newvariant.PurchaseToRecipeUom = null;    // to enable
                newvariant.ReorderPoint = null;           // to enable
                newvariant.MinQty = null;                 // to enable
                newvariant.MaxQty = null;                 // to enable

                newvariant.StatusId = model.StatusId;
                newvariant.CreateBy = model.ModifyBy;

                newvariant.ImagePath = _context.Companies.First().Id + "/Items";

                itemtoupdate.InverseVariantParent.Add(newvariant);
            }
        }


        //itemtoupdate.Sale = true; // to enable
        itemtoupdate.Price = model.Price;
        itemtoupdate.Cost = model.Cost;

        //itemtoupdate.Purchase = false;              // to enable
        //itemtoupdate.PurchaseUom = null;            // to enable
        //itemtoupdate.RecipeUom = null;              // to enable
        //itemtoupdate.PurchaseToRecipeUom = null;    // to enable
        //itemtoupdate.ReorderPoint = null;           // to enable
        //itemtoupdate.MinQty = null;                 // to enable
        //itemtoupdate.MaxQty = null;                 // to enable

        itemtoupdate.StatusId = model.StatusId;

        itemtoupdate.ModifyBy = model.ModifyBy;
        itemtoupdate.ModifyAt = DateTime.UtcNow;

        if (!itemtoupdate.HasVariants)
        {
            var DBItemNotInBrancheses = new List<ItemNotInBranch>(itemtoupdate.ItemNotInBranches);

            if (model.ItemNotInBranches == null)
            {
                model.ItemNotInBranches = new List<ItemNotInBranch>();
            }

            //Add if not in db
            foreach (var item in model.ItemNotInBranches)
            {
                if (!DBItemNotInBrancheses.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId))
                {
                    itemtoupdate.ItemNotInBranches.Add(item);
                }
            }
            //remove if in db and not in the list
            foreach (var item in DBItemNotInBrancheses)
            {
                if (!model.ItemNotInBranches.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId))
                {
                    itemtoupdate.ItemNotInBranches.Remove(item);
                }
            }

            var DBItemBranchPrices = new List<ItemBranchPrice>(itemtoupdate.ItemBranchPrices);

            if (model.ItemBranchPrices == null)
            {
                model.ItemBranchPrices = new List<ItemBranchPrice>();
            }

            //Add if not in db
            foreach (var item in model.ItemBranchPrices)
            {
                if (!DBItemBranchPrices.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId && x.Price == item.Price))
                {
                    itemtoupdate.ItemBranchPrices.Add(item);
                }
            }
            //remove if in db and not in the list
            foreach (var item in DBItemBranchPrices)
            {
                if (!model.ItemBranchPrices.Any(x => x.ItemId == item.ItemId && x.BranchId == item.BranchId && x.Price == item.Price))
                {
                    itemtoupdate.ItemBranchPrices.Remove(item);
                }
            }
        }

        if (oldPrice != model.Price)
        {
            if (!itemtoupdate.Modifier)
            {
                //var menuGroupItems = await _context.MenuGroupItems.Where(x => x.ItemId == itemtoupdate.Id && x.Price == oldPrice).ToListAsync();

                //foreach (var mgi in menuGroupItems)
                //{
                //    mgi.Price = model.Price;
                //}
            }
        }

        try
        {
            if (model.ImagePath == null)
            {
                itemtoupdate.ImagePath = _context.Companies.First().Id + "/Items";
            }

            if (model.ImageName == null)
            {
                if (itemtoupdate.ImageName != null)
                {
                    RemoveImage(itemtoupdate.ImagePath, itemtoupdate.ImageName);
                    itemtoupdate.ImageName = null;
                }
            }
            else if (model.ImageName != "")
            {
                RemoveImage(itemtoupdate.ImagePath, itemtoupdate.ImageName);
                itemtoupdate.ImageName = CreateImage(itemtoupdate.ImagePath, itemtoupdate.Id, model.ImageName);

            }
            _context.Entry(itemtoupdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return Ok(await _context.Items.Include(x => x.ItemGroup).Where(x => x.Id == model.Id).AsNoTracking().FirstOrDefaultAsync());

    }

    [Authorize(Roles = "Console, prm-items")]
    [HttpPost("Add")]
    public async Task<ActionResult<Item>> PostItem(Item model)
    {
        //var image = file;
        //get number series id from setting table
        var ns_id = _context.NumberSeriesSettings.Where(x => x.Id == "item").First().NumberSeriesId;
        //return Conflict();
        Item newitem = new Item();

        //get new id
        newitem.Id = Helpers.NumberSeriesHelper.get_new_id_for_insert(ns_id, _context, null, null);

        if (newitem.Id == null)
        {
            return Conflict();
        }

        newitem.Name = model.Name;
        newitem.Sname = model.Sname;

        newitem.ItemDivisionId = null; // to enable
        newitem.ItemCategoryId = null; // to enable + cascading to the division

        newitem.ItemGroupId = model.ItemGroupId;
        newitem.VatGroupId = model.VatGroupId;

        newitem.Barcode = null; // to enable

        newitem.Recipe = false;// to enable
        newitem.UseProduction = false;// to enable

        newitem.Modifier = model.Modifier;
        newitem.HasVariants = model.HasVariants;

        newitem.Description = model.Description;
        newitem.Sdescription = model.Sdescription;

        if (!newitem.Modifier)
        {
            if (model.ItemModifierItems == null)
            {
                model.ItemModifierItems = new List<ItemModifier>();
            }

            foreach (var item in model.ItemModifierItems)
            {
                newitem.ItemModifierItems.Add(new ItemModifier() { ModifierItemId = item.ModifierItemId });
            }

            //Modifier Groups
            if (model.ItemModifierGroups == null)
            {
                model.ItemModifierGroups = new List<ItemModifierGroup>();
            }

            foreach (var item in model.ItemModifierGroups)
            {
                var md = await _context.ModifierGroups.FirstOrDefaultAsync(x => x.Id == item.ModifierGroupId);
                if (md != null)
                {
                    newitem.ItemModifierGroups.Add(new ItemModifierGroup() { ModifierGroupId = md.Id, Min = md.Min, Max = md.Max, Free = 0, Multiple = md.Multiple });
                }
            }
        }


        if (newitem.HasVariants)
        {
            foreach (var var in model.InverseVariantParent)
            {
                var v_ns_id = _context.NumberSeriesSettings.Where(x => x.Id == "item").First().NumberSeriesId;

                Item newvariant = new Item();

                //get new id
                newvariant.Id = Helpers.NumberSeriesHelper.get_new_id_for_insert(ns_id, _context, null, null);

                if (newvariant.Id == null)
                {
                    return Conflict();
                }

                newvariant.Name = var.Name;
                newvariant.Sname = var.Sname;

                newvariant.ItemDivisionId = null; // to enable
                newvariant.ItemCategoryId = null; // to enable + cascading to the division

                newvariant.ItemGroupId = model.ItemGroupId;
                newvariant.VatGroupId = model.VatGroupId;

                newvariant.Barcode = null; // to enable

                newvariant.Recipe = false;// to enable
                newvariant.UseProduction = false;// to enable

                newvariant.Modifier = false;
                newvariant.HasVariants = false;

                newvariant.Sale = true; // to enable
                newvariant.Price = null;
                newvariant.Cost = 0;

                newvariant.Purchase = false;              // to enable
                newvariant.PurchaseUom = null;            // to enable
                newvariant.RecipeUom = null;              // to enable
                newvariant.PurchaseToRecipeUom = null;    // to enable
                newvariant.ReorderPoint = null;           // to enable
                newvariant.MinQty = null;                 // to enable
                newvariant.MaxQty = null;                 // to enable

                newvariant.StatusId = model.StatusId;
                newvariant.CreateBy = model.CreateBy;

                newvariant.ImagePath = _context.Companies.First().Id + "/Items";

                newitem.InverseVariantParent.Add(newvariant);

                if (model.ItemModifierGroups != null)
                {
                    foreach (var img in model.ItemModifierGroups)
                    {
                        newvariant.ItemModifierGroups.Add(new ItemModifierGroup()
                        {
                            ItemId = newvariant.Id,
                            ModifierGroupId = img.ModifierGroupId,
                            Min = img.Min,
                            Max = img.Max,
                            Free = img.Free,
                            Multiple = img.Multiple
                        });
                    }
                }
            }
        }

        if (!newitem.HasVariants)
        {
            //Add if not in db
            foreach (var item in model.ItemBranchPrices)
            {
                newitem.ItemBranchPrices.Add(item);
            }
        }
        newitem.Sale = true; // to enable
        newitem.Price = model.Price;
        newitem.Cost = 0;

        newitem.Purchase = false;              // to enable
        newitem.PurchaseUom = null;            // to enable
        newitem.RecipeUom = null;              // to enable
        newitem.PurchaseToRecipeUom = null;    // to enable
        newitem.ReorderPoint = null;           // to enable
        newitem.MinQty = null;                 // to enable
        newitem.MaxQty = null;                 // to enable

        newitem.ItemNotInBranches = model.ItemNotInBranches;

        newitem.StatusId = model.StatusId;
        newitem.CreateBy = model.CreateBy;

        newitem.ImagePath = _context.Companies.First().Id + "/Items";

        _context.Items.Add(newitem);
        try
        {
            await _context.SaveChangesAsync();
            if (model.ImageName != null)
            {
                newitem.ImageName = CreateImage(newitem.ImagePath, newitem.Id, model.ImageName);
                foreach (var var in newitem.InverseVariantParent)
                {
                    var.ImageName = newitem.ImageName;
                }

                _context.Entry(newitem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

        }
        catch (DbUpdateException)
        {
            if (ItemExists(newitem.Id))
            {
                return Conflict();
            }
            else
            {
                throw;
            }
        }

        return Ok(await _context.Items.Include(x => x.ItemGroup).Include(x => x.InverseVariantParent).Where(x => x.Id == newitem.Id).AsNoTracking().FirstOrDefaultAsync());

    }

 
    [Authorize(Roles = "Console, prm-items")]
    [HttpPut("Restore/{id}")]
    public async Task<ActionResult<Item>> RestoreItem(string id)
    {
        var item = await _context.Items.Include(x => x.InverseVariantParent).Where(x => x.Id == id).FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound();
        }

        try
        {
            item.StatusId = "st-active";

            if (item.HasVariants)
            {
                foreach (var variant in item.InverseVariantParent)
                {
                    variant.StatusId = "st-active";
                    variant.VariantParentId = item.Id;
                }
            }

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        catch (Exception ex)
        {
            return BadRequest();
        }
    }

    private bool ItemExists(string id)
    {
        return _context.Items.Any(e => e.Id == id);
    }

    [HttpGet("GetImage/{ImageName}")]
    public IActionResult GetItemImage(string ImageName)
    {
        try
        {
            string companyid = _context.Companies.First().Id;
            string webRootPath = _webHostEnvironment.WebRootPath;
            string path = Path.Combine(webRootPath, string.Format("Companies/{0}/items/{1}", companyid, ImageName));
            if (System.IO.File.Exists(path))
            {
                return Ok(Convert.ToBase64String(System.IO.File.ReadAllBytes(path)));
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpGet("GetFullImage/{ImageName}")]
    public IActionResult GetFullItemImage(string ImageName)
    {
        try
        {
            string companyid = _context.Companies.First().Id;
            string webRootPath = _webHostEnvironment.WebRootPath;
            string path = Path.Combine(webRootPath, string.Format("Companies/{0}/items/full/{1}", companyid, ImageName));
            if (System.IO.File.Exists(path))
            {
                return Ok(Convert.ToBase64String(System.IO.File.ReadAllBytes(path)));
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    private string CreateImage(string ImagePath, string id, string base64string)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;
        var datetime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        string path = Path.Combine(webRootPath, string.Format("Companies/{0}/{1}/{2}-{3}.jpg", ImagePath, "full", id, datetime));
        string Thumbnailpath = Path.Combine(webRootPath, string.Format("Companies/{0}/{1}-{2}.jpg", ImagePath, id, datetime));

        if (!Directory.Exists(webRootPath + "/Companies/" + ImagePath))
        {
            Directory.CreateDirectory(webRootPath + "/Companies/" + ImagePath);
        }
        if (!Directory.Exists(webRootPath + "/Companies/" + ImagePath + "/full"))
        {
            Directory.CreateDirectory(webRootPath + "/Companies/" + ImagePath + "/full");
        }
        //System.IO.File.WriteAllBytes(path, Convert.FromBase64String(base64string));


        var ItemImage = Utils.IOHelper.Base64ToImage(base64string);
        if (ItemImage != null)
        {
            ItemImage.Save(path);
            var imgthumnil = Utils.IOHelper.ResizeImage(ItemImage, new System.Drawing.Size(120, 120));
            //var ItemImageThumnil = ItemImage.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            imgthumnil.Save(Thumbnailpath);
        }
        else
        {
            //System.IO.File.WriteAllBytes(path, Convert.FromBase64String(base64string));
        }



        return string.Format("{0}-{1}.jpg", id, datetime);
    }

    private void RemoveImage(string ImagePath, string ImageName)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;

        string path = Path.Combine(webRootPath, string.Format("Companies/{0}/{1}/{2}", ImagePath, "full", ImageName));
        string Thumbnailpath = Path.Combine(webRootPath, string.Format("Companies/{0}/{1}", ImagePath, ImageName));

        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
        if (System.IO.File.Exists(Thumbnailpath))
        {
            System.IO.File.Delete(Thumbnailpath);
        }
    }

}
