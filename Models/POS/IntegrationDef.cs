using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class IntegrationDef
    {
        public IntegrationDef()
        {
            IntegrationBranchLevels = new HashSet<IntegrationBranchLevel>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<IntegrationBranchLevel> IntegrationBranchLevels { get; set; }
    }
}
