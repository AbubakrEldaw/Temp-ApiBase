using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class CompanyFeature
    {
        public string CompanyId { get; set; } = null!;
        public string FeatureId { get; set; } = null!;
        public bool? IsActive { get; set; }
        public bool InTrial { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public string SubscriptionCategoryId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual Company Company { get; set; } = null!;
        public virtual Feature Feature { get; set; } = null!;
        public virtual SubscriptionCategory SubscriptionCategory { get; set; } = null!;
    }
}
