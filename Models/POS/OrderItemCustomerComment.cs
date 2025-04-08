using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class OrderItemCustomerComment
    {
        public Guid OrderItemId { get; set; }
        public string CustomerCommentId { get; set; }

        public virtual CustomerComment CustomerComment { get; set; }
        public virtual OrderItem OrderItem { get; set; }
    }
}
