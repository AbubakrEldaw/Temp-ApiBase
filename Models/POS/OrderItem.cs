using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class OrderItem
    {
        public OrderItem()
        {
            InverseModifierParentNavigation = new HashSet<OrderItem>();
            OrderItemCustomerComments = new HashSet<OrderItemCustomerComment>();
        }

        public byte[] RowVersion { get; set; }
        public Guid Id { get; set; }
        public Guid OrderHeaderId { get; set; }
        public string MenuGroupId { get; set; }
        public string ItemId { get; set; }
        public string VariantId { get; set; }
        public string ItemDescription { get; set; }
        public bool Modifier { get; set; }
        public Guid? ModifierParent { get; set; }
        public string ModifierGroupId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public decimal HeaderDiscountAmount { get; set; }
        public bool? PriceVatInclusive { get; set; }
        public decimal VatAmount { get; set; }
        public string DiscountId { get; set; }
        public string DiscountTypeId { get; set; }
        public decimal? DiscountValue { get; set; }
        public string VatGroupId { get; set; }
        public decimal VatPercentage { get; set; }
        public bool? KotPrinted { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public bool Void { get; set; }
        public string VoidBy { get; set; }
        public DateTime? VoidAt { get; set; }
        public string VoidTypeId { get; set; }
        public string Note { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual Discount Discount { get; set; }
        public virtual DiscountType DiscountType { get; set; }
        public virtual Item Item { get; set; }
        public virtual ModifierGroup ModifierGroup { get; set; }
        public virtual OrderItem ModifierParentNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual OrderHeader OrderHeader { get; set; }
        public virtual Item Variant { get; set; }
        public virtual VatGroup VatGroup { get; set; }
        public virtual Employee VoidByNavigation { get; set; }
        public virtual VoidType VoidType { get; set; }
        public virtual ICollection<OrderItem> InverseModifierParentNavigation { get; set; }
        public virtual ICollection<OrderItemCustomerComment> OrderItemCustomerComments { get; set; }
    }
}
