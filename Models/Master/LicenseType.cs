using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class LicenseType
    {
        public LicenseType()
        {
            CompanyLicenses = new HashSet<CompanyLicense>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
    }
}
