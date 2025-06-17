using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class CompanyBranch
    {
        public string CompanyId { get; set; } = null!;
        public string PosBranchId { get; set; } = null!;
        public Guid GlobalBranchId { get; set; }

        public virtual Company Company { get; set; } = null!;
        public virtual CompanyBranchIntegration CompanyBranchIntegration { get; set; } = null!;
    }
}
