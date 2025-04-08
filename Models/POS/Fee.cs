using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Fee
    {
        public Fee()
        {
            FeeItems = new HashSet<FeeItem>();
            OrderFees = new HashSet<OrderFee>();
        }

        public byte[] RowVersion { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string VatGroupId { get; set; }
        public string FeeTypeId { get; set; }
        public decimal Value { get; set; }
        public string StatusId { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public decimal MinCharge { get; set; }
        public bool ApplyPerUnit { get; set; }
        public bool ApplyOnItem { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual FeeType FeeType { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual Status Status { get; set; }
        public virtual VatGroup VatGroup { get; set; }
        public virtual ICollection<FeeItem> FeeItems { get; set; }
        public virtual ICollection<OrderFee> OrderFees { get; set; }
    }
}
