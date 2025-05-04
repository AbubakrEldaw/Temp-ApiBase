using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ItemModifierGroup
    {
        public string ModifierGroupId { get; set; }
        public string ItemId { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Free { get; set; }
        public bool Multiple { get; set; }

        public virtual Item Item { get; set; }
        public virtual ModifierGroup ModifierGroup { get; set; }
    }
}
