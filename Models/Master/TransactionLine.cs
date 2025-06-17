using System;
using System.Collections.Generic;

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
        /// <summary>
        /// License, Feature, App
        /// </summary>
        public string Type { get; set; } = null!;
        /// <summary>
        /// License, Feature, or App Id
        /// </summary>
        public string ReferenceId { get; set; } = null!;
        /// <summary>
        /// Complete name of the purchased item
        /// </summary>
        public string Description { get; set; } = null!;
        public string Sdescription { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AutoRenew { get; set; }
        public string PlanId { get; set; } = null!;

        public virtual Plan Plan { get; set; } = null!;
        public virtual Transaction Transaction { get; set; } = null!;
        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
    }
}
