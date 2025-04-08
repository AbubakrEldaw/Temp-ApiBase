using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class TransactionLine
    {
        public TransactionLine()
        {
            CompanyLicenses = new HashSet<CompanyLicense>();
        }

        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public int OrderIndex { get; set; }
        public string Type { get; set; }
        public string ReferenceId { get; set; }
        public string Description { get; set; }
        public string Sdescription { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }

        public virtual Transaction Transaction { get; set; }
        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
    }
}
