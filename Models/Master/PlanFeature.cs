using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class PlanFeature
    {
        public string PlanId { get; set; }
        public string FeatureId { get; set; }

        public virtual Feature Feature { get; set; }
        public virtual Plan Plan { get; set; }
    }
}
