using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Item
    {
        public Item()
        {
            FeeItems = new HashSet<FeeItem>();
            InverseVariantParent = new HashSet<Item>();
            ItemBranchPrices = new HashSet<ItemBranchPrice>();
            ItemDiscounts = new HashSet<ItemDiscount>();
            ItemModifierGroups = new HashSet<ItemModifierGroup>();
            ItemModifierItems = new HashSet<ItemModifier>();
            ItemModifierModifierItems = new HashSet<ItemModifier>();
            ItemNotInBranches = new HashSet<ItemNotInBranch>();
            KitchenPrintGroupItems = new HashSet<KitchenPrintGroupItem>();
            ModifierGroupItems = new HashSet<ModifierGroupItem>();
            OrderItemItems = new HashSet<OrderItem>();
            OrderItemVariants = new HashSet<OrderItem>();
        }

        public byte[] RowVersion { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string ItemDivisionId { get; set; }
        public string ItemCategoryId { get; set; }
        public string ItemGroupId { get; set; }
        public string VatGroupId { get; set; }
        public string Barcode { get; set; }
        public string PurchaseUom { get; set; }
        public string RecipeUom { get; set; }
        public int? PurchaseToRecipeUom { get; set; }
        public bool Recipe { get; set; }
        public bool Modifier { get; set; }
        public bool HasVariants { get; set; }
        public string VariantParentId { get; set; }
        public bool UseProduction { get; set; }
        public bool Purchase { get; set; }
        public bool Sale { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public bool? SaleByWeight { get; set; }
        public int? ReorderPoint { get; set; }
        public int? MinQty { get; set; }
        public int? MaxQty { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string StatusId { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public string Description { get; set; }
        public string Sdescription { get; set; }
        public string Nutrition { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual ItemCategory ItemCategory { get; set; }
        public virtual ItemDivision ItemDivision { get; set; }
        public virtual ItemGroup ItemGroup { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual Status Status { get; set; }
        public virtual Item VariantParent { get; set; }
        public virtual VatGroup VatGroup { get; set; }
        public virtual ICollection<FeeItem> FeeItems { get; set; }
        public virtual ICollection<Item> InverseVariantParent { get; set; }
        public virtual ICollection<ItemBranchPrice> ItemBranchPrices { get; set; }
        public virtual ICollection<ItemDiscount> ItemDiscounts { get; set; }
        public virtual ICollection<ItemModifierGroup> ItemModifierGroups { get; set; }
        public virtual ICollection<ItemModifier> ItemModifierItems { get; set; }
        public virtual ICollection<ItemModifier> ItemModifierModifierItems { get; set; }
        public virtual ICollection<ItemNotInBranch> ItemNotInBranches { get; set; }
        public virtual ICollection<KitchenPrintGroupItem> KitchenPrintGroupItems { get; set; }
        public virtual ICollection<ModifierGroupItem> ModifierGroupItems { get; set; }
        public virtual ICollection<OrderItem> OrderItemItems { get; set; }
        public virtual ICollection<OrderItem> OrderItemVariants { get; set; }
    }
}
