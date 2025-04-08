using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class OrderFee
    {
        public Guid OrderHeaderId { get; set; }
        public string FeeId { get; set; }
        public string FeeTypeId { get; set; }
        public decimal FeeValue { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
        public bool? PriceVatInclusive { get; set; }
        public string VatGroupId { get; set; }
        public decimal VatPercentage { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }

        public virtual Fee Fee { get; set; }
        public virtual FeeType FeeType { get; set; }
        public virtual OrderHeader OrderHeader { get; set; }
    }
}
