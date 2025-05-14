using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Discount
    {
        public Discount()
        {
            DiscountCustomerGroups = new HashSet<DiscountCustomerGroup>();
            ItemDiscounts = new HashSet<ItemDiscount>();
            OrderHeaders = new HashSet<OrderHeader>();
            OrderItems = new HashSet<OrderItem>();
            ItemDiscounts = new HashSet<ItemDiscount>();
        }

        public byte[] RowVersion { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string DiscountTypeId { get; set; }
        public decimal Value { get; set; }
        public int Priority { get; set; }
        public bool WholeOrder { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public bool AutoApply { get; set; }
        public string StatusId { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public bool RequireCustomer { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual DiscountType DiscountType { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<DiscountCustomerGroup> DiscountCustomerGroups { get; set; }
        public virtual ICollection<ItemDiscount> ItemDiscounts { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<ItemDiscount> ItemDiscounts { get; set; }
    }
}
