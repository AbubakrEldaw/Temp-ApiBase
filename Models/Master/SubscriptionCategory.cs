using System;
using System.Collections.Generic;

#nullable disable

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

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<CompanyApp> CompanyApps { get; set; }
        public virtual ICollection<CompanyFeature> CompanyFeatures { get; set; }
        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
    }
}
