using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Payment
    {
        public Payment()
        {
            OrderPayments = new HashSet<OrderPayment>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string PaymentTypeId { get; set; }
        public bool OpenCashDrawer { get; set; }
        public string StatusId { get; set; }
        public bool RequireReferenceNumber { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual PaymentType PaymentType { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<OrderPayment> OrderPayments { get; set; }
    }
}
