using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class DisplayFeaturePlan
    {
        public string DisplayFeatureId { get; set; }
        public string PlanId { get; set; }

        public virtual DisplayFeature DisplayFeature { get; set; }
        public virtual Plan Plan { get; set; }
    }
}
