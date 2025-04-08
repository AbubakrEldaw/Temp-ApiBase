using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class VatGroup
    {
        public VatGroup()
        {
            Branches = new HashSet<Branch>();
            Fees = new HashSet<Fee>();
            Items = new HashSet<Item>();
            OrderItems = new HashSet<OrderItem>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public int Percentage { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual ICollection<Branch> Branches { get; set; }
        public virtual ICollection<Fee> Fees { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
