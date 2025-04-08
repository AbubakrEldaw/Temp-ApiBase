using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Country
    {
        public Country()
        {
            Cities = new HashSet<City>();
            CustomerAddresses = new HashSet<CustomerAddress>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Code { get; set; }

        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; }
    }
}
