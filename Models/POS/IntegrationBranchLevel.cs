using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class IntegrationBranchLevel
    {
        public string IntId { get; set; }
        public string BranchId { get; set; }
        public string JsonValue { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual IntegrationDef Int { get; set; }
    }
}
