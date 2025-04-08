using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class VoidType
    {
        public VoidType()
        {
            OrderHeaders = new HashSet<OrderHeader>();
            OrderItems = new HashSet<OrderItem>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
