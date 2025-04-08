using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Customer
    {
        public Customer()
        {
            CustomerAddresses = new HashSet<CustomerAddress>();
            CustomerCustomerGroups = new HashSet<CustomerCustomerGroup>();
            CustomerLoyaltyTransactions = new HashSet<CustomerLoyaltyTransaction>();
            OrderHeaders = new HashSet<OrderHeader>();
        }

        public byte[] RowVersion { get; set; }
        public Guid Id { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int TotalVisits { get; set; }
        public decimal? TotalSpent { get; set; }
        public DateTime? FirstVisit { get; set; }
        public DateTime? LastVisit { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public string Nat { get; set; }
        public string VatRegNo { get; set; }
        public string Crn { get; set; }
        public string RegistrationName { get; set; }
        public string StreetName { get; set; }
        public string BuildingNumber { get; set; }
        public string PlotIdentification { get; set; }
        public string CitySubdivisionName { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string CountrySubentity { get; set; }
        public string Country { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; }
        public virtual ICollection<CustomerCustomerGroup> CustomerCustomerGroups { get; set; }
        public virtual ICollection<CustomerLoyaltyTransaction> CustomerLoyaltyTransactions { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
    }
}
