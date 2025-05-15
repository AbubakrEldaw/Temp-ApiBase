using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class CompanyBranchSubescriptionLicense
    {
        public string CompanyId { get; set; }
        public string BranchId { get; set; }
        public string LicenseTypeId { get; set; }
        public int Quantity { get; set; }

        public virtual CompanyBranchSubescription CompanyBranchSubescription { get; set; }
        public virtual LicenseType LicenseType { get; set; }
    }
}
