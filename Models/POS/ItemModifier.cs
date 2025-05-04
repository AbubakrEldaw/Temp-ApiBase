using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ItemModifier
    {
        public string ItemId { get; set; }
        public string ModifierItemId { get; set; }

        public virtual Item Item { get; set; }
        public virtual Item ModifierItem { get; set; }
    }
}
