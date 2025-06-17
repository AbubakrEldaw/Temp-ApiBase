using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class CompanyPlan
    {
        public string CompanyId { get; set; } = null!;
        public string PlanId { get; set; } = null!;
        public bool InTrial { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool? IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int GracePeriod { get; set; }
        public byte? RenewalAttempts { get; set; }

        public virtual Company Company { get; set; } = null!;
        public virtual Plan Plan { get; set; } = null!;
    }
}
