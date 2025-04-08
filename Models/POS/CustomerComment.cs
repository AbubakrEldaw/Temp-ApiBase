using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class CustomerComment
    {
        public CustomerComment()
        {
            OrderItemCustomerComments = new HashSet<OrderItemCustomerComment>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<OrderItemCustomerComment> OrderItemCustomerComments { get; set; }
    }
}
