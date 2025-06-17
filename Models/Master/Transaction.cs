using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class Transaction
    {
        public Transaction()
        {
            BankTransfers = new HashSet<BankTransfer>();
            PaymentSessions = new HashSet<PaymentSession>();
            TransactionLines = new HashSet<TransactionLine>();
            TransactionPayments = new HashSet<TransactionPayment>();
        }

        public Guid Id { get; set; }
        public string CompanyId { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int? PaidBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Status { get; set; } = null!;
        public string Type { get; set; } = null!;

        public virtual Company Company { get; set; } = null!;
        public virtual User CreatedByNavigation { get; set; } = null!;
        public virtual User? ModifiedByNavigation { get; set; }
        public virtual User? PaidByNavigation { get; set; }
        public virtual ICollection<BankTransfer> BankTransfers { get; set; }
        public virtual ICollection<PaymentSession> PaymentSessions { get; set; }
        public virtual ICollection<TransactionLine> TransactionLines { get; set; }
        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; }
    }
}
