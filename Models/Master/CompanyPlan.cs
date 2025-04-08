using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class CompanyPlan
    {
        public string CompanyId { get; set; }
        public string PlanId { get; set; }
        public bool InTrial { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int GracePeriod { get; set; }

        public virtual Company Company { get; set; }
        public virtual Plan Plan { get; set; }
    }
}
