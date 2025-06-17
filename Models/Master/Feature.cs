using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class Feature
    {
        public Feature()
        {
            CompanyFeatures = new HashSet<CompanyFeature>();
            Plans = new HashSet<Plan>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;
        public string? Description { get; set; }
        public string? Sdescription { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public bool? IsPurchasable { get; set; }

        public virtual ICollection<CompanyFeature> CompanyFeatures { get; set; }

        public virtual ICollection<Plan> Plans { get; set; }
    }
}
