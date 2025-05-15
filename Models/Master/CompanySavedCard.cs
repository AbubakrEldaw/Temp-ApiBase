using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class CompanySavedCard
    {
        public Guid Id { get; set; }
        public Guid Alias { get; set; }
        public Guid DisplayToken { get; set; }
        public Guid? SubscriptionToken { get; set; }
        public string CompanyId { get; set; }
        public string CardNumberMasked { get; set; }
        public string Brand { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public bool IsDefault { get; set; }

        public virtual Company Company { get; set; }
    }
}
