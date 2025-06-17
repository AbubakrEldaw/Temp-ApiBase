using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class CompanyBranchIntegration
    {
        public Guid GlobalBranchId { get; set; }
        public string? FoodizoneBranchId { get; set; }

        public virtual CompanyBranch GlobalBranch { get; set; } = null!;
    }
}
