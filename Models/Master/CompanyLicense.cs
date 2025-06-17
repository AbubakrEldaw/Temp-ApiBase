using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class CompanyLicense
    {
        public CompanyLicense()
        {
            InverseParent = new HashSet<CompanyLicense>();
        }

        public Guid Id { get; set; }
        public string CompanyId { get; set; } = null!;
        public string LicenseTypeId { get; set; } = null!;
        public bool InTrial { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public string SubscriptionCategoryId { get; set; } = null!;
        public string? LinkId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? TransactionLineId { get; set; }

        public virtual Company Company { get; set; } = null!;
        public virtual LicenseType LicenseType { get; set; } = null!;
        public virtual CompanyLicense? Parent { get; set; }
        public virtual SubscriptionCategory SubscriptionCategory { get; set; } = null!;
        public virtual TransactionLine? TransactionLine { get; set; }
        public virtual ICollection<CompanyLicense> InverseParent { get; set; }
    }
}
