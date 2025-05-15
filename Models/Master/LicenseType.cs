using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class LicenseType
    {
        public LicenseType()
        {
            CompanyBranchSubescriptionLicenses = new HashSet<CompanyBranchSubescriptionLicense>();
            CompanyLicenses = new HashSet<CompanyLicense>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public virtual ICollection<CompanyBranchSubescriptionLicense> CompanyBranchSubescriptionLicenses { get; set; }
        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
    }
}
