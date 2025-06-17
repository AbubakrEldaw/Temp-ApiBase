using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class TransactionPayment
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public string PaymentMethodId { get; set; } = null!;
        public int OrderIndex { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CompanySavedCardId { get; set; }

        public virtual CompanySavedCard? CompanySavedCard { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
        public virtual Transaction Transaction { get; set; } = null!;
    }
}
