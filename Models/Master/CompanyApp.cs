using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class CompanyApp
    {
        public string CompanyId { get; set; }
        public string MarketPlaceAppId { get; set; }
        public string JsonProp { get; set; }
        public bool? IsActive { get; set; }
        public bool InTrial { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public string SubscriptionCategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual Company Company { get; set; }
        public virtual MarketPlaceApp MarketPlaceApp { get; set; }
        public virtual SubscriptionCategory SubscriptionCategory { get; set; }
    }
}
