using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ItemNotInBranch
    {
        public string ItemId { get; set; }
        public string BranchId { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual Item Item { get; set; }
    }
}
