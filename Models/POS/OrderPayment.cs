using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class OrderPayment
    {
        public Guid OrderHeaderId { get; set; }
        public int LineIndex { get; set; }
        public string PaymentId { get; set; }
        public string PaymentTypeDescription { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Amount { get; set; }

        public virtual OrderHeader OrderHeader { get; set; }
        public virtual Payment Payment { get; set; }
    }
}
