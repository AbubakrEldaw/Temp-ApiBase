using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class FeeItem
    {
        public string FeeId { get; set; }
        public string ItemId { get; set; }

        public virtual Fee Fee { get; set; }
        public virtual Item Item { get; set; }
    }
}
