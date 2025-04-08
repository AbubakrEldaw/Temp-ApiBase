using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class DeliveryZone
    {
        public DeliveryZone()
        {
            CustomerAddresses = new HashSet<CustomerAddress>();
        }

        public string Id { get; set; }
        public string CityId { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual City City { get; set; }
        public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; }
    }
}
