using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class KitchenPrintGroupItem
    {
        public string KitchenPrintGroupId { get; set; }
        public string ItemId { get; set; }

        public virtual Item Item { get; set; }
        public virtual KitchenPrintGroup KitchenPrintGroup { get; set; }
    }
}
