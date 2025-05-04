using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ItemBranchPrice
    {
        public string BranchId { get; set; }
        public string ItemId { get; set; }
        public decimal Price { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual Item Item { get; set; }
    }
}
