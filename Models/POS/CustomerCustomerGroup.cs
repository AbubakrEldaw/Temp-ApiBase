using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class CustomerCustomerGroup
    {
        public Guid CustomerId { get; set; }
        public string CustomerGroupId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual CustomerGroup CustomerGroup { get; set; }
    }
}
