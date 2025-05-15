using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class PaymentSession
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string CompanyId { get; set; }
        public Guid? TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentJson { get; set; }
        public string TransactionLinesJson { get; set; }

        public virtual Company Company { get; set; }
        public virtual Transaction Transaction { get; set; }
        public virtual User User { get; set; }
    }
}
