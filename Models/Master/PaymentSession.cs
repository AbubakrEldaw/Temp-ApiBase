using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class PaymentSession
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string CompanyId { get; set; } = null!;
        public Guid? TransactionId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string PaymentJson { get; set; } = null!;
        public string TransactionLinesJson { get; set; } = null!;

        public virtual Company Company { get; set; } = null!;
        public virtual Transaction? Transaction { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
