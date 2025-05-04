using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class DiscountCustomerGroup
    {
        public string DiscountId { get; set; }
        public string CustomerGroupId { get; set; }

        public virtual CustomerGroup CustomerGroup { get; set; }
        public virtual Discount Discount { get; set; }
    }
}
