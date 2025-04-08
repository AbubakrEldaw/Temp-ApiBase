using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class AggrInfo
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string Email { get; set; }
        public string Pwd { get; set; }
        public string Token { get; set; }
        public string Text { get; set; }
        public string OrderSourceId { get; set; }
        public string DiningOptionId { get; set; }
        public string CashPaymentId { get; set; }
        public string CreditPaymentId { get; set; }
        public string UnmapedItemId { get; set; }
        public string UnmapedModifierId { get; set; }
        public string UnmapedCustomerCommentId { get; set; }
        public bool Active { get; set; }
        public bool? Pay { get; set; }
        public string PayAs { get; set; }
    }
}
