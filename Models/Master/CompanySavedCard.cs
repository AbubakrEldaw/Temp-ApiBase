using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class CompanySavedCard
    {
        public CompanySavedCard()
        {
            TransactionPayments = new HashSet<TransactionPayment>();
        }

        public Guid Id { get; set; }
        public Guid Alias { get; set; }
        public Guid DisplayToken { get; set; }
        public Guid? SubscriptionToken { get; set; }
        public string CompanyId { get; set; } = null!;
        public string CardNumberMasked { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string ExpiryMonth { get; set; } = null!;
        public string ExpiryYear { get; set; } = null!;
        public bool IsDefault { get; set; }

        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; }
    }
}
