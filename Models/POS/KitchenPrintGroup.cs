using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class KitchenPrintGroup
    {
        public KitchenPrintGroup()
        {
            KitchenPrintGroupItems = new HashSet<KitchenPrintGroupItem>();
            KitchenPrintGroupPosPrinters = new HashSet<KitchenPrintGroupPosPrinter>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string BranchId { get; set; }
        public int PrintLanguage { get; set; }
        public string AdvancedRouting { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual ICollection<KitchenPrintGroupItem> KitchenPrintGroupItems { get; set; }
        public virtual ICollection<KitchenPrintGroupPosPrinter> KitchenPrintGroupPosPrinters { get; set; }
    }
}
