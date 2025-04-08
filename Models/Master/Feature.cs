using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class Feature
    {
        public Feature()
        {
            CompanyFeatures = new HashSet<CompanyFeature>();
            PlanFeatures = new HashSet<PlanFeature>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Description { get; set; }
        public string Sdescription { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public bool? IsPurchasable { get; set; }

        public virtual ICollection<CompanyFeature> CompanyFeatures { get; set; }
        public virtual ICollection<PlanFeature> PlanFeatures { get; set; }
    }
}
