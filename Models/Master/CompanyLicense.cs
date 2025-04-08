using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class CompanyLicense
    {
        public CompanyLicense()
        {
            InverseParent = new HashSet<CompanyLicense>();
        }

        public Guid Id { get; set; }
        public string CompanyId { get; set; }
        public string LicenseTypeId { get; set; }
        public bool InTrial { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public string SubscriptionCategoryId { get; set; }
        public string LinkId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? TransactionLineId { get; set; }

        public virtual Company Company { get; set; }
        public virtual LicenseType LicenseType { get; set; }
        public virtual CompanyLicense Parent { get; set; }
        public virtual SubscriptionCategory SubscriptionCategory { get; set; }
        public virtual TransactionLine TransactionLine { get; set; }
        public virtual ICollection<CompanyLicense> InverseParent { get; set; }
    }
}
