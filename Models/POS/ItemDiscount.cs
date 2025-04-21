using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ItemDiscount
    {
        public string ItemId { get; set; }
        public string DiscountId { get; set; }

        public virtual Discount Discount { get; set; }
        public virtual Item Item { get; set; }
    }
}
