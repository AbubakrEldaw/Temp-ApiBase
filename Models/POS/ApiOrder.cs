using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ApiOrder
    {
        public ApiOrder()
        {
            ApiOrderStatusHistories = new HashSet<ApiOrderStatusHistory>();
        }

        public byte[] RowVersion { get; set; }
        public Guid Id { get; set; }
        public string BranchId { get; set; }
        public string GlobalLocationId { get; set; }
        public int OrderType { get; set; }
        public bool OrderIsPaid { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string PaymentType { get; set; }
        public string OrderSource { get; set; }
        public string OrderModel { get; set; }
        public string StagingStatusId { get; set; }
        public string AppId { get; set; }
        public string AppOrderId { get; set; }
        public string AppOrderNumber { get; set; }
        public DateTime AppOrderReceiveDatetime { get; set; }
        public DateTime AppOrderPickupDatetime { get; set; }
        public Guid? PosOrderId { get; set; }
        public string PosOrderNumber { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual ApiOrderStagingStatus StagingStatus { get; set; }
        public virtual ICollection<ApiOrderStatusHistory> ApiOrderStatusHistories { get; set; }
    }
}
