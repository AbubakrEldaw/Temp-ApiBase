using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class PaymentMethod
    {
        public PaymentMethod()
        {
            TransactionPayments = new HashSet<TransactionPayment>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;
        public string? Description { get; set; }
        public string? Sdescription { get; set; }
        public decimal Fees { get; set; }
        public string Status { get; set; } = null!;

        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; }
    }
}
