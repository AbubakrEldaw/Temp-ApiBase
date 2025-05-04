using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ModifierGroup
    {
        public ModifierGroup()
        {
            ItemModifierGroups = new HashSet<ItemModifierGroup>();
            ModifierGroupItems = new HashSet<ModifierGroupItem>();
            OrderItems = new HashSet<OrderItem>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int? Free { get; set; }
        public bool Multiple { get; set; }
        public int OrderIndex { get; set; }

        public virtual ICollection<ItemModifierGroup> ItemModifierGroups { get; set; }
        public virtual ICollection<ModifierGroupItem> ModifierGroupItems { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
