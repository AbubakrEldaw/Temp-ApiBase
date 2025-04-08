using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class City
    {
        public City()
        {
            CustomerAddresses = new HashSet<CustomerAddress>();
            DeliveryZones = new HashSet<DeliveryZone>();
        }

        public string Id { get; set; }
        public string CountryId { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; }
        public virtual ICollection<DeliveryZone> DeliveryZones { get; set; }
    }
}
