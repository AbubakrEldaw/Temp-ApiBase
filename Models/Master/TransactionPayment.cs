using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class TransactionPayment
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public string PaymentMethodId { get; set; }
        public int OrderIndex { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual PaymentMethod PaymentMethod { get; set; }
        public virtual Transaction Transaction { get; set; }
    }
}
