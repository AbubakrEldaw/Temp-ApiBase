using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class CustomerLoyaltyTransaction
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid OrderHeaderId { get; set; }
        public string Type { get; set; }
        public int Points { get; set; }
        public decimal? BaseAmount { get; set; }
        public decimal? Rate { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
