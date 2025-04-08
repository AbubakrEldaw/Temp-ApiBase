using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class CustomerGroup
    {
        public CustomerGroup()
        {
            CustomerCustomerGroups = new HashSet<CustomerCustomerGroup>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<CustomerCustomerGroup> CustomerCustomerGroups { get; set; }
    }
}
