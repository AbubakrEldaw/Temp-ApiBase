using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class CompanyBranch
    {
        public string CompanyId { get; set; }
        public string PosBranchId { get; set; }
        public Guid GlobalBranchId { get; set; }

        public virtual Company Company { get; set; }
        public virtual CompanyBranchIntegration CompanyBranchIntegration { get; set; }
        public virtual CompanyBranchSubescription CompanyBranchSubescription { get; set; }
    }
}
