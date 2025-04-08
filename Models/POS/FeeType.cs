using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class FeeType
    {
        public FeeType()
        {
            Fees = new HashSet<Fee>();
            OrderFees = new HashSet<OrderFee>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<Fee> Fees { get; set; }
        public virtual ICollection<OrderFee> OrderFees { get; set; }
    }
}
