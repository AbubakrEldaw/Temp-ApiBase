using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class PosPrinter
    {
        public PosPrinter()
        {
            KitchenPrintGroupPosPrinters = new HashSet<KitchenPrintGroupPosPrinter>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string BranchId { get; set; }
        public string Model { get; set; }
        public string TcpIp { get; set; }
        public string PrinterName { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual Employee CreateByNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual ICollection<KitchenPrintGroupPosPrinter> KitchenPrintGroupPosPrinters { get; set; }
    }
}
