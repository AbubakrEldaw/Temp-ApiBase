using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class Plan
    {
        public Plan()
        {
            CompanyPlans = new HashSet<CompanyPlan>();
            DisplayFeaturePlans = new HashSet<DisplayFeaturePlan>();
            MarketPlaceAppPlanAvailabilities = new HashSet<MarketPlaceAppPlanAvailability>();
            PlanFeatures = new HashSet<PlanFeature>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Description { get; set; }
        public string Sdescription { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public virtual ICollection<CompanyPlan> CompanyPlans { get; set; }
        public virtual ICollection<DisplayFeaturePlan> DisplayFeaturePlans { get; set; }
        public virtual ICollection<MarketPlaceAppPlanAvailability> MarketPlaceAppPlanAvailabilities { get; set; }
        public virtual ICollection<PlanFeature> PlanFeatures { get; set; }
    }
}
