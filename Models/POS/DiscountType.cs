using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class DiscountType
    {
        public DiscountType()
        {
            Discounts = new HashSet<Discount>();
            OrderItems = new HashSet<OrderItem>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<Discount> Discounts { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
