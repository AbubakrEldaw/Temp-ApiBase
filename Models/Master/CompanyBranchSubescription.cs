using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class CompanyBranchSubescription
    {
        public CompanyBranchSubescription()
        {
            CompanyBranchSubescriptionLicenses = new HashSet<CompanyBranchSubescriptionLicense>();
        }

        public string CompanyId { get; set; }
        public string BranchId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int GracePeriodInDays { get; set; }

        public virtual Company Company { get; set; }
        public virtual CompanyBranch CompanyBranch { get; set; }
        public virtual ICollection<CompanyBranchSubescriptionLicense> CompanyBranchSubescriptionLicenses { get; set; }
    }
}
