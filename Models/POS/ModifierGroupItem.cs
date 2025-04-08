using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ModifierGroupItem
    {
        public string ModifierGroupId { get; set; }
        public string ItemId { get; set; }
        public decimal Price { get; set; }
        public int OrderIndex { get; set; }

        public virtual Item Item { get; set; }
        public virtual ModifierGroup ModifierGroup { get; set; }
    }
}
