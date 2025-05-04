using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class KitchenPrintGroupPosPrinter
    {
        public string KitchenPrintGroupId { get; set; }
        public string PosPrinterId { get; set; }

        public virtual KitchenPrintGroup KitchenPrintGroup { get; set; }
        public virtual PosPrinter PosPrinter { get; set; }
    }
}
