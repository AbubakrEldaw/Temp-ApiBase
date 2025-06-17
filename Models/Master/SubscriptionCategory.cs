using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class SubscriptionCategory
    {
        public SubscriptionCategory()
        {
            CompanyApps = new HashSet<CompanyApp>();
            CompanyFeatures = new HashSet<CompanyFeature>();
            CompanyLicenses = new HashSet<CompanyLicense>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;

        public virtual ICollection<CompanyApp> CompanyApps { get; set; }
        public virtual ICollection<CompanyFeature> CompanyFeatures { get; set; }
        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
    }
}
