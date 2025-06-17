using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class Plan
    {
        public Plan()
        {
            CompanyPlans = new HashSet<CompanyPlan>();
            TransactionLines = new HashSet<TransactionLine>();
            DisplayFeatures = new HashSet<DisplayFeature>();
            Features = new HashSet<Feature>();
            MarketPlaceApps = new HashSet<MarketPlaceApp>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;
        public string? Description { get; set; }
        public string? Sdescription { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public virtual ICollection<CompanyPlan> CompanyPlans { get; set; }
        public virtual ICollection<TransactionLine> TransactionLines { get; set; }

        public virtual ICollection<DisplayFeature> DisplayFeatures { get; set; }
        public virtual ICollection<Feature> Features { get; set; }
        public virtual ICollection<MarketPlaceApp> MarketPlaceApps { get; set; }
    }
}
