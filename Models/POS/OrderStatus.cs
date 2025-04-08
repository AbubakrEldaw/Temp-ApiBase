using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class OrderStatus
    {
        public OrderStatus()
        {
            OrderHeaders = new HashSet<OrderHeader>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public int? OrderIndex { get; set; }

        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
    }
}
