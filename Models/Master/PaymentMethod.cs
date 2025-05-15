using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class PaymentMethod
    {
        public PaymentMethod()
        {
            TransactionPayments = new HashSet<TransactionPayment>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Description { get; set; }
        public string Sdescription { get; set; }
        public decimal Fees { get; set; }
        public string Status { get; set; }

        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; }
    }
}
